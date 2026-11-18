using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Creates Sqlite contexts for migration tooling without connecting to the database.
/// </summary>
public sealed class SqliteDataContextFactory : IDesignTimeDbContextFactory<SqliteDataContext>
{
    /// <summary>
    /// Creates a caller-owned Sqlite context with local development connection settings.
    /// </summary>
    /// <param name="args">
    /// Tooling arguments, which are currently unused.
    /// </param>
    /// <returns>
    /// The context to dispose after the design-time operation.
    /// </returns>
    public SqliteDataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqliteDataContext>()
            .UseSqlite("Data Source=smurfs-sqlite.db")
            .Options;

        return new SqliteDataContext(options);
    }
}
