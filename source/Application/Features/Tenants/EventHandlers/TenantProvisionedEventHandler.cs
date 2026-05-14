using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Tenants.EventHandlers;

/// <summary>
/// Handles the TenantProvisionedEvent by seeding default data for newly created tenants.
/// Seeds system roles (Admin, User) with default permissions.
/// </summary>
public sealed class TenantProvisionedEventHandler : INotificationHandler<TenantProvisionedEvent>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TenantProvisionedEventHandler(
        IRoleRepository roleRepository, 
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TenantProvisionedEvent notification, CancellationToken cancellationToken)
    {
        var tenantIdResult = TenantId.Create(notification.TenantId);
        if (tenantIdResult.IsFailure)
        {
            // Log error - tenant ID should always be valid at this point
            return;
        }

        var tenantId = tenantIdResult.Value;

        // Create default system roles for the new tenant
        await SeedSystemRolesAsync(tenantId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSystemRolesAsync(TenantId tenantId, CancellationToken ct)
    {
        // Create Admin role with full permissions
        var adminRoleResult = Role.Create(tenantId, "Admin", isSystemRole: true);
        if (adminRoleResult.IsSuccess)
        {
            var adminRole = adminRoleResult.Value;
            adminRole.AssignPermission("users.read");
            adminRole.AssignPermission("users.write");
            adminRole.AssignPermission("users.delete");
            adminRole.AssignPermission("roles.read");
            adminRole.AssignPermission("roles.write");
            adminRole.AssignPermission("roles.delete");
            adminRole.AssignPermission("tenant.read");
            adminRole.AssignPermission("tenant.write");

            await _roleRepository.AddAsync(adminRole, ct);
        }

        // Create User role with limited permissions
        var userRoleResult = Role.Create(tenantId, "User", isSystemRole: true);
        if (userRoleResult.IsSuccess)
        {
            var userRole = userRoleResult.Value;
            userRole.AssignPermission("users.read");
            userRole.AssignPermission("tenant.read");

            await _roleRepository.AddAsync(userRole, ct);
        }

        // Create Viewer role with read-only permissions
        var viewerRoleResult = Role.Create(tenantId, "Viewer", isSystemRole: true);
        if (viewerRoleResult.IsSuccess)
        {
            var viewerRole = viewerRoleResult.Value;
            viewerRole.AssignPermission("users.read");
            viewerRole.AssignPermission("tenant.read");

            await _roleRepository.AddAsync(viewerRole, ct);
        }
    }
}
