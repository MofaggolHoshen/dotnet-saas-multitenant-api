using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IUserRepository users,
    IPasswordHashingService passwordHasher,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result<RegisterResponse>.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var tenantIdResult = TenantId.Create(tenantContext.TenantId);
        if (tenantIdResult.IsFailure)
        {
            return Result<RegisterResponse>.Failure(tenantIdResult.Error);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result<RegisterResponse>.Failure(emailResult.Error);
        }

        var existingUser = await users.GetByEmailAsync(tenantIdResult.Value, emailResult.Value.Value, ct);
        if (existingUser is not null)
        {
            return Result<RegisterResponse>.Failure(Error.Conflict("A user with this email already exists."));
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        var userResult = User.Create(tenantIdResult.Value, emailResult.Value, passwordHash, request.FullName);
        if (userResult.IsFailure)
        {
            return Result<RegisterResponse>.Failure(userResult.Error);
        }

        await users.AddAsync(userResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<RegisterResponse>.Success(new RegisterResponse(
            UserId: userResult.Value.Id,
            Email: userResult.Value.Email.Value,
            FullName: userResult.Value.FullName,
            Message: "Registration successful."));
    }
}
