using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Exposes dependency injection registration for EF Core app-data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the selected provider context, shared context and factory contracts, and scoped database initializer.
    /// </summary>
    public static IServiceCollection AddAppDataPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
        where TContext : DataContext
    {
        services.AddDbContextFactory<TContext>(optionsAction);
        services.AddSingleton<IDbContextFactory<DataContext>, AppDataContextFactory<TContext>>();
        services.AddScoped<DataContext>(provider => provider.GetRequiredService<TContext>());
        services.AddScoped<IAppDataInitializer, AppDataInitializer<TContext>>();

        // register repositories, unit of work, etc.
        //services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

