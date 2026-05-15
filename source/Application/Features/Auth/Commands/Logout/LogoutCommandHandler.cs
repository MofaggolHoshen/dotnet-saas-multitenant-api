using Application.Common.Interfaces;
using Domain.Common;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var token = await refreshTokens.GetByTokenAsync(request.RefreshToken, ct);

        if (token is null)
        {
            // Treat a missing token as success — already effectively logged out
            return Result.Success();
        }

        if (token.IsActive)
        {
            token.Revoke();
            await refreshTokens.UpdateAsync(token, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
