using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Smurfs.Application.Core;

/// <summary>
/// Exposes dependency injection registration for the application use cases.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the assembly's MediatR handlers and the request failure logging and validation pipeline behaviors.
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Smurfs.List.Handler).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestFailureLoggingBehavior<,>));

        return services;
    }
}
