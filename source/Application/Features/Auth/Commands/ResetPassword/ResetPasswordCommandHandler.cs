using Domain.Common;
using MediatR;

namespace Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Stub handler — full implementation requires a reset token store and email service.
/// TODO: Validate reset token from persistent store, update password hash, revoke all refresh tokens.
/// </summary>
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        // TODO: Look up reset token in store, verify it is valid and not expired.
        // TODO: Find user associated with the reset token.
        // TODO: Hash new password and call user.UpdatePassword().
        // TODO: Revoke all active refresh tokens for the user.
        // TODO: Delete / invalidate the used reset token.

        return Task.FromResult(Result.Failure(Error.Validation("Password reset via token is not yet implemented.")));
    }
}
