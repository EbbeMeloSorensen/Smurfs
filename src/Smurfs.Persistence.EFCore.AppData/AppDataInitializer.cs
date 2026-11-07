using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Runs application database setup using the supplied EF Core context.
/// </summary>
public class AppDataInitializer<TContext> : IAppDataInitializer
    where TContext : DataContext
{
    private readonly TContext _context;

    /// <summary>
    /// Uses the supplied context for migrations and sample data seeding.
    /// </summary>
    public AppDataInitializer(TContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Applies outstanding EF Core migrations for the context.
    /// </summary>
    public Task MigrateAsync() => _context.Database.MigrateAsync();

    /// <summary>
    /// Adds sample Smurfs when the migrated Smurfs table is empty.
    /// </summary>
    public Task SeedAsync() => Seeding.SeedDatabase(_context);
}
