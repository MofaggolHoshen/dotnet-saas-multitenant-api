using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
