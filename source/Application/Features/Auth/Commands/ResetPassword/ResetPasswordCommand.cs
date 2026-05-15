using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Resets a user's password using a valid one-time reset token.
/// Full implementation requires token persistence and validation (future phase).
/// </summary>
public sealed record ResetPasswordCommand(
    string ResetToken,
    string NewPassword,
    string ConfirmNewPassword)
    : ICommand<Result>;
