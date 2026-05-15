using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IUserRepository users,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result<UserDto>.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null || user.TenantId.Value != tenantContext.TenantId)
        {
            return Result<UserDto>.Failure(Error.NotFound("User not found."));
        }

        var updateResult = user.UpdateProfile(request.FullName);
        if (updateResult.IsFailure)
        {
            return Result<UserDto>.Failure(updateResult.Error);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<UserDto>.Success(new UserDto(
            Id: user.Id,
            Email: user.Email.Value,
            FullName: user.FullName,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAtUtc));
    }
}
