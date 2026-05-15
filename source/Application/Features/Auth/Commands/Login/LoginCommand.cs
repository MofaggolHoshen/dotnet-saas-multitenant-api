using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password, string? TenantIdentifier = null)
    : ICommand<Result<LoginResponse>>;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Email,
    string FullName);
