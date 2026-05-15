using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Identity;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
        => TryParseGuid(httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier));

    public string? Email
        => httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public Guid? TenantId
        => TryParseGuid(httpContextAccessor.HttpContext?.User?.FindFirstValue("tenantId"));

    public bool IsAuthenticated
        => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private static Guid? TryParseGuid(string? value)
        => Guid.TryParse(value, out var id) ? id : null;
}
