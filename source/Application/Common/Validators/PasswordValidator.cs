using System.Text.RegularExpressions;

namespace Application.Common.Validators;

public static partial class PasswordValidator
{
    private const int MinimumLength = 8;

    public static bool IsValid(string password, out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return false;
        }

        if (password.Length < MinimumLength)
        {
            errors.Add($"Password must be at least {MinimumLength} characters long.");
        }

        if (!UppercaseRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!LowercaseRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (!DigitRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one number.");
        }

        if (!SpecialCharRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one special character.");
        }

        return errors.Count == 0;
    }

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]")]
    private static partial Regex SpecialCharRegex();
}
