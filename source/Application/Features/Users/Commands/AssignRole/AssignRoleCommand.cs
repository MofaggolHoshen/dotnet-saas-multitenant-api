using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Users.Commands.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, string RoleName) : ICommand<Result>;
