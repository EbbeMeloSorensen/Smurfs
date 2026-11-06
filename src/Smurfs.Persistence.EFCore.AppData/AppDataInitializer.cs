using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

public class AppDataInitializer<TContext> : IAppDataInitializer
    where TContext : DataContext
{
    private readonly TContext _context;

    public AppDataInitializer(TContext context)
    {
        _context = context;
    }

    public Task MigrateAsync() => _context.Database.MigrateAsync();

    public Task SeedAsync() => Seeding.SeedDatabase(_context);
}
