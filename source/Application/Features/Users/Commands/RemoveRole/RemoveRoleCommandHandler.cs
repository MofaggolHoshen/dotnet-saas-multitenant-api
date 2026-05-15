using Application.Common.Interfaces;
using Domain.Common;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Users.Commands.RemoveRole;

public sealed class RemoveRoleCommandHandler(
    IUserRepository users,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveRoleCommand, Result>
{
    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null || user.TenantId.Value != tenantContext.TenantId)
        {
            return Result.Failure(Error.NotFound("User not found."));
        }

        var removeResult = user.RemoveRole(request.RoleId);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
