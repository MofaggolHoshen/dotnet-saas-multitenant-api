using Application.Common.Interfaces;
using Domain.Common;
using Domain.ValueObjects;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Stub handler — full implementation requires an email notification service.
/// For now, responds with success regardless of whether the email exists
/// to prevent user enumeration.
/// TODO: Integrate email service and persist reset token (future phase).
/// </summary>
public sealed class ForgotPasswordCommandHandler(
    IUserRepository users,
    ITenantContext tenantContext) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result.Success();
        }

        var tenantIdResult = TenantId.Create(tenantContext.TenantId);
        if (tenantIdResult.IsFailure)
        {
            return Result.Success();
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Success();
        }

        // Look up user — but always return Success to prevent user enumeration
        _ = await users.GetByEmailAsync(tenantIdResult.Value, emailResult.Value.Value, ct);

        // TODO: If user exists, generate a secure reset token, persist it, and send via email.

        return Result.Success();
    }
}
