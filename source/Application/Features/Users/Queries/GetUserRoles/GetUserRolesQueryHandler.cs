using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Users.Queries.GetUserRoles;

public sealed class GetUserRolesQueryHandler(
    IUserRepository users,
    IRoleRepository roles,
    ITenantContext tenantContext) : IRequestHandler<GetUserRolesQuery, Result<List<UserRoleDto>>>
{
    public async Task<Result<List<UserRoleDto>>> Handle(GetUserRolesQuery request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result<List<UserRoleDto>>.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null || user.TenantId.Value != tenantContext.TenantId)
        {
            return Result<List<UserRoleDto>>.Failure(Error.NotFound("User not found."));
        }

        if (user.RoleIds.Count == 0)
        {
            return Result<List<UserRoleDto>>.Success([]);
        }

        var roleEntities = await roles.GetByIdsAsync(user.RoleIds, ct);
        var roleDtos = roleEntities
            .Select(r => new UserRoleDto(r.Id, r.Name))
            .ToList();

        return Result<List<UserRoleDto>>.Success(roleDtos);
    }
}
