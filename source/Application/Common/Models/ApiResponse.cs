namespace Application.Common.Models;

/// <summary>
/// Unified API response wrapper for both successful and failed operations.
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the data returned by a successful operation, or null if the operation failed.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets an optional message describing the result.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the list of errors that occurred during a failed operation.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Creates a successful API response with data.
    /// </summary>
    /// <param name="data">The data to return.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>A successful API response.</returns>
    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    /// <summary>
    /// Creates a failed API response with an error message.
    /// </summary>
    /// <param name="message">The primary error message.</param>
    /// <param name="errors">Additional error details.</param>
    /// <returns>A failed API response.</returns>
    public static ApiResponse<T> Fail(string message, params string[] errors) => new()
    {
        Success = false,
        Message = message,
        Errors = errors.ToList()
    };
}
