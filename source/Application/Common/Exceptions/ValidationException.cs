using FluentValidation.Results;

namespace Application.Common.Exceptions;

/// <summary>
/// Exception thrown when one or more validation failures occur during request processing.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    /// <summary>
    /// Gets the validation errors grouped by property name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
