using Application.Common.Interfaces;
using Domain.Common;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(
    IUserRepository users,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateUserCommand, Result>
{
    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken ct)
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

        var deactivateResult = user.Deactivate();
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
