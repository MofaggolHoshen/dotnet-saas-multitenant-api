using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;

namespace Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName
) : ICommand<Result<UserDto>>;
