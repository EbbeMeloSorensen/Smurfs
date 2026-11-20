using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Registers the selected Identity provider context through its shared context contract.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the provider context and exposes the same scoped instance to Identity stores and startup migration.
    /// </summary>
    /// <typeparam name="TContext">
    /// The provider context that owns the selected migration set.
    /// </typeparam>
    /// <param name="services">
    /// The application's service registrations.
    /// </param>
    /// <param name="optionsAction">
    /// Configures the provider and its connection.
    /// </param>
    /// <returns>
    /// The service collection for further registration.
    /// </returns>
    public static IServiceCollection AddIdentityPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
        where TContext : DataContext
    {
        services.AddDbContext<TContext>(optionsAction);
        services.AddScoped<DataContext>(provider => provider.GetRequiredService<TContext>());
        return services;
    }
}
