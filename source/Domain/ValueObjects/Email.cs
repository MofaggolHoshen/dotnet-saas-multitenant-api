using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex =
        new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<Email>.Failure(Error.Validation("Email is required."));
        }

        var normalized = input.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
        {
            return Result<Email>.Failure(Error.Validation("Email format is invalid."));
        }

        return Result<Email>.Success(new Email(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
