using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Initiates the password reset flow by issuing a one-time reset token.
/// The token should be sent to the user's email address.
/// Full implementation requires an email notification service (future phase).
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand<Result>;
