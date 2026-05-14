using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Multitenancy;

/// <summary>
/// Middleware that resolves and attaches tenant context before request handlers execute.
/// Bypasses tenant resolution for health checks, authentication, and Swagger endpoints.
/// </summary>
public sealed class TenantMiddleware
{
    private static readonly string[] BypassPrefixes = 
    [
        "/health", 
        "/swagger", 
        "/api/v1/auth",
        "/_health",
        "/_configuration"
    ];

    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context, 
        ITenantResolver resolver, 
        TenantContext tenantContext)
    {
        // Bypass tenant resolution for specific endpoints
        if (ShouldBypassTenantResolution(context))
        {
            await _next(context);
            return;
        }

        // Attempt to resolve tenant from request
        var resolved = await resolver.ResolveTenantAsync(context, context.RequestAborted);
        
        if (resolved is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                new { error = "Tenant not specified or invalid." }, 
                context.RequestAborted);
            return;
        }

        // Set tenant context for current request
        tenantContext.SetTenant(resolved.Value.TenantId, resolved.Value.TenantName);
        
        await _next(context);
    }

    private static bool ShouldBypassTenantResolution(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return BypassPrefixes.Any(prefix => 
            path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Extension methods for registering TenantMiddleware in the application pipeline.
/// </summary>
public static class TenantMiddlewareExtensions
{
    /// <summary>
    /// Adds tenant resolution middleware to the application pipeline.
    /// Should be called after UseRouting() and before UseAuthorization().
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}
