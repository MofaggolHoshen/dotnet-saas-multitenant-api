using Domain.Common;

namespace Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    private RefreshToken(Guid userId, string token, DateTime expiresAtUtc) : base()
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAtUtc)
        => new(userId, token, expiresAtUtc);

    public void Revoke()
    {
        if (!IsRevoked)
        {
            RevokedAtUtc = DateTime.UtcNow;
            MarkUpdated();
        }
    }
}
