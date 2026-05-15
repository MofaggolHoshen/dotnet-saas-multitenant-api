using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword)
    : ICommand<Result>;
