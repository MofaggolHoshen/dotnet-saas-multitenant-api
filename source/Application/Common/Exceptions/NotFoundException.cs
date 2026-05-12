namespace Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity cannot be found.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
        EntityName = name;
        Key = key;
    }

    /// <summary>
    /// Gets the name of the entity that was not found.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the key of the entity that was not found.
    /// </summary>
    public object Key { get; }
}
