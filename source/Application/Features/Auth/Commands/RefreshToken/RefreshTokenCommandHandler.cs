using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    IRoleRepository roles,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private const int AccessTokenExpirationMinutes = 30;
    private const int RefreshTokenExpirationDays = 7;

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existingToken = await refreshTokens.GetByTokenAsync(request.RefreshToken, ct);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure(Error.Conflict("Refresh token is invalid or has expired."));
        }

        var user = await users.GetByIdAsync(existingToken.UserId, ct);
        if (user is null || !user.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure(Error.Conflict("User not found or is inactive."));
        }

        // Revoke the consumed token (rotation — one token per use)
        existingToken.Revoke();
        await refreshTokens.UpdateAsync(existingToken, ct);

        var roleNames = await GetRoleNamesAsync(user.RoleIds, ct);
        var newAccessToken = tokenGenerator.GenerateAccessToken(user, roleNames);
        var newRefreshTokenValue = tokenGenerator.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

        var newRefreshToken = Domain.Entities.RefreshToken.Create(user.Id, newRefreshTokenValue, newRefreshTokenExpiry);
        await refreshTokens.AddAsync(newRefreshToken, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshTokenValue,
            ExpiresAtUtc: DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes)));
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
