using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Pipeline behaviors will be added in Phase 12
            // cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            // cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        return services;
    }
}
