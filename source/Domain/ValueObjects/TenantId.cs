using Domain.Common;

namespace Domain.ValueObjects;

public sealed class TenantId : ValueObject
{
    public Guid Value { get; }

    private TenantId(Guid value)
    {
        Value = value;
    }

    public static Result<TenantId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result<TenantId>.Failure(Error.Validation("TenantId cannot be empty."));
        }

        return Result<TenantId>.Success(new TenantId(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
