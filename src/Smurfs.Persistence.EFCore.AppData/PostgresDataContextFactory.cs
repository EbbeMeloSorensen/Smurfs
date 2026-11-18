using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Creates Postgres contexts for migration tooling without connecting to the database.
/// </summary>
public sealed class PostgresDataContextFactory : IDesignTimeDbContextFactory<PostgresDataContext>
{
    /// <summary>
    /// Creates a caller-owned Postgres context with local development connection settings.
    /// </summary>
    /// <param name="args">
    /// Tooling arguments, which are currently unused.
    /// </param>
    /// <returns>
    /// The context to dispose after the design-time operation.
    /// </returns>
    public PostgresDataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PostgresDataContext>()
            .UseNpgsql("Host=localhost;Port=5432;Username=root;Password=root;Database=Smurfs_API")
            .Options;

        return new PostgresDataContext(options);
    }
}
