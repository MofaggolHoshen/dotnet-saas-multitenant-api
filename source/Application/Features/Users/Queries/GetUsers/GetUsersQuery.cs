using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Users.DTOs;
using Domain.Common;

namespace Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool? IsActive = null
) : IQuery<Result<PaginatedList<UserDto>>>;
