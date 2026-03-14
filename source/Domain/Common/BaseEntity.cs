namespace Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? UpdatedAtUtc { get; protected set; }
    public bool IsDeleted { get; protected set; }

    protected BaseEntity(Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkUpdated() => UpdatedAtUtc = DateTime.UtcNow;
    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}