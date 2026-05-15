using Application.Features.Users.Commands.AssignRole;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.DeactivateUser;
using Application.Features.Users.Commands.RemoveRole;
using Application.Features.Users.Commands.UpdateUser;
using Application.Features.Users.Queries.GetUserById;
using Application.Features.Users.Queries.GetUserRoles;
using Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Authorize]
public sealed class UsersController(ISender sender) : ApiController(sender)
{
    /// <summary>
    /// Returns a paginated list of users for the current tenant.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetUsersQuery(pageNumber, pageSize, isActive), ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Returns detailed information for a specific user.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserById(Guid userId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetUserByIdQuery(userId), ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Returns the roles assigned to a specific user.
    /// </summary>
    [HttpGet("{userId:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetUserRolesQuery(userId), ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new user within the current tenant.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetUserById), new { userId = result.Value.Id }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateUserCommand(userId, request.FullName), ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivates a user within the current tenant.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeactivateUser(Guid userId, CancellationToken ct)
    {
        var result = await Sender.Send(new DeactivateUserCommand(userId), ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    [HttpPost("{userId:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new AssignRoleCommand(userId, request.RoleName), ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken ct)
    {
        var result = await Sender.Send(new RemoveRoleCommand(userId, roleId), ct);
        return HandleResult(result);
    }
}

public sealed record UpdateUserRequest(string FullName);
public sealed record AssignRoleRequest(string RoleName);
