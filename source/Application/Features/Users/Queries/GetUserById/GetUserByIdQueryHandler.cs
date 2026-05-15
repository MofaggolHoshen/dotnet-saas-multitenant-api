using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(
    IUserRepository users,
    IRoleRepository roles,
    ITenantContext tenantContext) : IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
    public async Task<Result<UserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result<UserDetailDto>.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null || user.TenantId.Value != tenantContext.TenantId)
        {
            return Result<UserDetailDto>.Failure(Error.NotFound("User not found."));
        }

        var userRoles = new List<UserRoleDto>();
        if (user.RoleIds.Count > 0)
        {
            var roleEntities = await roles.GetByIdsAsync(user.RoleIds, ct);
            userRoles = roleEntities
                .Select(r => new UserRoleDto(r.Id, r.Name))
                .ToList();
        }

        return Result<UserDetailDto>.Success(new UserDetailDto(
            Id: user.Id,
            Email: user.Email.Value,
            FullName: user.FullName,
            IsActive: user.IsActive,
            TenantId: user.TenantId.Value,
            TenantName: null,
            Roles: userRoles,
            CreatedAt: user.CreatedAtUtc,
            UpdatedAt: user.UpdatedAtUtc));
    }
}
