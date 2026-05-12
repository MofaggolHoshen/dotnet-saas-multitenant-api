namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction for getting the current date and time.
/// Allows for deterministic testing by injecting a fake implementation.
/// </summary>
public interface IDateTime
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
