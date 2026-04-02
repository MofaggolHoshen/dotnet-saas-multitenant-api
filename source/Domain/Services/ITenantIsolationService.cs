using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Services;

public interface ITenantIsolationService
{
    bool CanAccess(User actor, TenantId tenantId);
    bool IsIsolated(TenantId tenantId);
    Task<bool> ValidateTenantAccessAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
}
