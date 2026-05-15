using Domain.Services;

namespace Infrastructure.Identity;

public sealed class PasswordHasher : IPasswordHashingService
{
    public string Hash(string plainTextPassword)
        => BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor: 12);

    public bool Verify(string plainTextPassword, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
}
