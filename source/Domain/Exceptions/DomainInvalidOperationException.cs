namespace Domain.Exceptions;

public sealed class DomainInvalidOperationException : DomainException
{
    public DomainInvalidOperationException(string message)
        : base(message)
    {
    }

    public DomainInvalidOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
