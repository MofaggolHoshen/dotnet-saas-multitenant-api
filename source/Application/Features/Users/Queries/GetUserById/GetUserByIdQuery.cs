using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;

namespace Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<Result<UserDetailDto>>;
