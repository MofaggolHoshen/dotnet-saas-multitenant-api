using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IPasswordHashingService passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenRepository refreshTokens,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private const int AccessTokenExpirationMinutes = 30;
    private const int RefreshTokenExpirationDays = 7;

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result<LoginResponse>.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var tenantIdResult = TenantId.Create(tenantContext.TenantId);
        if (tenantIdResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(tenantIdResult.Error);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(emailResult.Error);
        }

        var user = await users.GetByEmailAsync(tenantIdResult.Value, emailResult.Value.Value, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash) || !user.IsActive)
        {
            // Return the same error regardless of which check failed to prevent user enumeration
            return Result<LoginResponse>.Failure(Error.Conflict("Invalid credentials."));
        }

        var roleNames = await GetRoleNamesAsync(user.RoleIds, ct);
        var accessToken = tokenGenerator.GenerateAccessToken(user, roleNames);
        var refreshTokenValue = tokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

        var refreshToken = Domain.Entities.RefreshToken.Create(user.Id, refreshTokenValue, refreshTokenExpiry);
        await refreshTokens.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<LoginResponse>.Success(new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            ExpiresAtUtc: DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            UserId: user.Id,
            Email: user.Email.Value,
            FullName: user.FullName));
    }

    private async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken ct)
    {
        if (roleIds.Count == 0)
        {
            return Array.Empty<string>();
        }

        var userRoles = await roles.GetByIdsAsync(roleIds, ct);
        return userRoles.Select(r => r.Name).ToList();
    }
}
