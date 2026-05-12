namespace Application.Common.Models;

/// <summary>
/// Represents detailed error information for API responses.
/// </summary>
/// <param name="Code">The error code identifying the type of error.</param>
/// <param name="Message">A human-readable error message.</param>
/// <param name="TraceId">An optional trace identifier for debugging.</param>
public sealed record ErrorDetails(string Code, string Message, string? TraceId = null);
