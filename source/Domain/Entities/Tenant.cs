using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Tenant : AggregateRoot
{
    private Tenant(string name, string subdomain, SubscriptionTier tier) : base()
    {
        Name = name;
        Subdomain = subdomain;
        Tier = tier;
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Subdomain { get; private set; }
    public SubscriptionTier Tier { get; private set; }
    public bool IsActive { get; private set; }

    public static Result<Tenant> Create(string name, string subdomain, SubscriptionTier tier)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Tenant>.Failure(Error.Validation("Tenant name is required."));
        }

        if (string.IsNullOrWhiteSpace(subdomain) || subdomain.Contains(' '))
        {
            return Result<Tenant>.Failure(Error.Validation("Subdomain is invalid."));
        }

        var tenant = new Tenant(name.Trim(), subdomain.Trim().ToLowerInvariant(), tier);
        tenant.AddDomainEvent(new TenantProvisionedEvent(tenant.Id, tenant.Name, tenant.Subdomain));
        return Result<Tenant>.Success(tenant);
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    public Result UpdateSettings(string name, SubscriptionTier tier)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("Tenant name cannot be empty."));
        }

        Name = name.Trim();
        Tier = tier;
        MarkUpdated();
        return Result.Success();
    }
}
