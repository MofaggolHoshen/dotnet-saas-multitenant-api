using Application.Common.Interfaces;
using Domain.Common;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Users.Commands.AssignRole;

public sealed class AssignRoleCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignRoleCommand, Result>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var tenantIdResult = TenantId.Create(tenantContext.TenantId);
        if (tenantIdResult.IsFailure)
        {
            return Result.Failure(tenantIdResult.Error);
        }

        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null || user.TenantId.Value != tenantContext.TenantId)
        {
            return Result.Failure(Error.NotFound("User not found."));
        }

        var role = await roles.GetByNameAsync(tenantIdResult.Value, request.RoleName, ct);
        if (role is null)
        {
            return Result.Failure(Error.NotFound($"Role '{request.RoleName}' not found."));
        }

        var assignResult = user.AssignRole(role.Id);
        if (assignResult.IsFailure)
        {
            return assignResult;
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
