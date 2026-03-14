using Domain.Common;

namespace Domain.ValueObjects;

public sealed class Password : ValueObject
{
    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Result<Password> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<Password>.Failure(Error.Validation("Password is required."));
        }

        if (input.Length < 8)
        {
            return Result<Password>.Failure(Error.Validation("Password must be at least 8 characters."));
        }

        if (!input.Any(char.IsUpper) || !input.Any(char.IsLower) || !input.Any(char.IsDigit))
        {
            return Result<Password>.Failure(Error.Validation("Password must contain upper, lower, and digit."));
        }

        return Result<Password>.Success(new Password(input));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
