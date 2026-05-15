using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName)
    : ICommand<Result<RegisterResponse>>;

public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Message);
