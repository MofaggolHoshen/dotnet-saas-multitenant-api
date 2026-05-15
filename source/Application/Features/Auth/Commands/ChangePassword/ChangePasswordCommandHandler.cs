using Application.Common.Interfaces;
using Domain.Common;
using Domain.Repositories;
using Domain.Services;
using MediatR;

namespace Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHashingService passwordHasher,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure(Error.Validation("User is not authenticated."));
        }

        var user = await users.GetByIdAsync(currentUser.UserId.Value, ct);
        if (user is null || !user.IsActive)
        {
            return Result.Failure(Error.NotFound("User not found or is inactive."));
        }

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(Error.Conflict("Current password is incorrect."));
        }

        var newPasswordHash = passwordHasher.Hash(request.NewPassword);
        var updateResult = user.UpdatePassword(newPasswordHash);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        users.Update(user);

        // Revoke all active refresh tokens — force re-login on other devices
        await refreshTokens.RevokeAllUserTokensAsync(user.Id, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
