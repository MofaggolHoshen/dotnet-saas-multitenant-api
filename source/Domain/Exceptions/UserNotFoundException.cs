namespace Domain.Exceptions;

public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid userId)
        : base($"User '{userId}' was not found.")
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}
