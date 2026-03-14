using Domain.Common;

namespace Domain.ValueObjects;

public sealed class SubscriptionTier : ValueObject
{
    public static readonly SubscriptionTier Free = new("Free", 5, false);
    public static readonly SubscriptionTier Pro = new("Pro", 50, true);
    public static readonly SubscriptionTier Enterprise = new("Enterprise", int.MaxValue, true);

    private SubscriptionTier(string name, int maxUsers, bool supportsCustomRoles)
    {
        Name = name;
        MaxUsers = maxUsers;
        SupportsCustomRoles = supportsCustomRoles;
    }

    public string Name { get; }
    public int MaxUsers { get; }
    public bool SupportsCustomRoles { get; }

    public static Result<SubscriptionTier> Create(string input)
    {
        return input?.Trim().ToLowerInvariant() switch
        {
            "free" => Result<SubscriptionTier>.Success(Free),
            "pro" => Result<SubscriptionTier>.Success(Pro),
            "enterprise" => Result<SubscriptionTier>.Success(Enterprise),
            _ => Result<SubscriptionTier>.Failure(Error.Validation("Unsupported subscription tier."))
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    public override string ToString() => Name;
}
