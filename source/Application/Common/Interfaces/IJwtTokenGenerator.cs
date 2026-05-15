using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IReadOnlyCollection<string> roles);
    string GenerateRefreshToken();
}
