using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smurfs.Persistence.EFCore.Identity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(optionsAction);

        return services;
    }
}
