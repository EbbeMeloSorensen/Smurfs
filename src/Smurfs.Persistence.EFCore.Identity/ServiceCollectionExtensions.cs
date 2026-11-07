using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Exposes dependency injection registration for the Identity database context.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the requested EF Core context using caller-supplied options.
    /// </summary>
    public static IServiceCollection AddIdentityPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(optionsAction);

        return services;
    }
}
