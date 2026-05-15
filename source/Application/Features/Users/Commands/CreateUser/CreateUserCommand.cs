using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Domain.Common;

namespace Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user.
/// </summary>
public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FullName,
    List<string>? InitialRoleNames = null
) : ICommand<Result<UserDto>>;
