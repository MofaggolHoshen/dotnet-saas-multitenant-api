using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand<Result>;
