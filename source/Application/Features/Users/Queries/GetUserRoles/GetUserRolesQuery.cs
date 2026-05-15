using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;

namespace Application.Features.Users.Queries.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<Result<List<UserRoleDto>>>;
