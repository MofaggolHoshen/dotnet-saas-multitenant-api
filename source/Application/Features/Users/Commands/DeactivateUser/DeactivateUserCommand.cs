using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : ICommand<Result>;
