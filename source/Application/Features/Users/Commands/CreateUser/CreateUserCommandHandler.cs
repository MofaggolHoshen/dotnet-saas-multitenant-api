using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IPasswordHashingService passwordHasher,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result<UserDto>.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var tenantIdResult = TenantId.Create(tenantContext.TenantId);
        if (tenantIdResult.IsFailure)
        {
            return Result<UserDto>.Failure(tenantIdResult.Error);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result<UserDto>.Failure(emailResult.Error);
        }

        var existingUser = await users.GetByEmailAsync(tenantIdResult.Value, emailResult.Value.Value, ct);
        if (existingUser is not null)
        {
            return Result<UserDto>.Failure(Error.Conflict($"User with email '{request.Email}' already exists."));
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        var userResult = User.Create(tenantIdResult.Value, emailResult.Value, passwordHash, request.FullName);
        if (userResult.IsFailure)
        {
            return Result<UserDto>.Failure(userResult.Error);
        }

        var user = userResult.Value;

        if (request.InitialRoleNames is not null && request.InitialRoleNames.Count > 0)
        {
            foreach (var roleName in request.InitialRoleNames)
            {
                var role = await roles.GetByNameAsync(tenantIdResult.Value, roleName, ct);
                if (role is null)
                {
                    return Result<UserDto>.Failure(Error.NotFound($"Role '{roleName}' not found."));
                }

                var assignResult = user.AssignRole(role.Id);
                if (assignResult.IsFailure)
                {
                    return Result<UserDto>.Failure(assignResult.Error);
                }
            }
        }

        await users.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<UserDto>.Success(new UserDto(
            Id: user.Id,
            Email: user.Email.Value,
            FullName: user.FullName,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAtUtc));
    }
}
