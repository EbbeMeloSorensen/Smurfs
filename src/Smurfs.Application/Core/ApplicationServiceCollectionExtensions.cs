using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Smurfs.Application.Core;

/// <summary>
/// Exposes dependency injection registration for the application use cases.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the assembly's MediatR handlers and the validation pipeline behavior.
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Smurfs.List.Handler).Assembly));

        //services.AddMediatR(cfg =>
        //    cfg.RegisterServicesFromAssembly(typeof(Smurfs.Create.Handler).Assembly));

        //services.AddValidatorsFromAssembly(typeof(Smurfs.Create.CommandValidator).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
