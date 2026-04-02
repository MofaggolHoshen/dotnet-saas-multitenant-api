namespace Domain.Exceptions;

public sealed class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException(string email)
        : base($"User with email '{email}' already exists.")
    {
        Email = email;
    }

    public string Email { get; }
}
