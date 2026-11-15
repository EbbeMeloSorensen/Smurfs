using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Creates a DataContext for Entity Framework Core design-time operations,
/// such as generating and applying migrations.
/// </summary>
public sealed class DataContextFactory
    : IDesignTimeDbContextFactory<DataContext>
{
    /// <summary>
    /// Creates the SQLite Identity context for design-time tooling.
    /// </summary>
    public DataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            //.UseSqlite("Data Source=smurfs-identity.db")
            .UseNpgsql("Server=localhost;Port=5432;User Id=root;Password=root;Database=Smurfs_API")
            .Options;

        return new DataContext(options);
    }
}
