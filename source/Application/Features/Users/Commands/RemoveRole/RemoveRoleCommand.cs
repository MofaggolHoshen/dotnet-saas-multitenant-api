using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Features.Users.Commands.RemoveRole;

public sealed record RemoveRoleCommand(Guid UserId, Guid RoleId) : ICommand<Result>;
