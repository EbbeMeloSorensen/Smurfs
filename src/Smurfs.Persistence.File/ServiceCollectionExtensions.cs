using Microsoft.Extensions.DependencyInjection;
using Smurfs.Application.Smurfs;

namespace Smurfs.Persistence.File;

/// <summary>
/// Registers the file persistence implementations for a host.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers independent sessions, the initializer, and the read service for one file path.
    /// Relative paths are resolved at registration time.
    /// </summary>
    public static IServiceCollection AddFilePersistence(this IServiceCollection services, string filePath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var path = Path.GetFullPath(filePath);
        services.AddSingleton<IUnitOfWorkFactory>(new UnitOfWorkFactory(path));
        services.AddSingleton<IAppDataInitializer>(new AppDataInitializer(path));
        services.AddScoped<ISmurfReadService, Smurfs.SmurfReadService>();
        return services;
    }
}
