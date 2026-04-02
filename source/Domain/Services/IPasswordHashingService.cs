namespace Domain.Services;

public interface IPasswordHashingService
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string passwordHash);
}
