using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Provides the shared catalog model with the Sqlite migration history.
/// </summary>
public sealed class SqliteDataContext : DataContext
{
    /// <summary>
    /// Creates a context using the caller's Sqlite provider options.
    /// </summary>
    /// <param name="options">
    /// The provider and connection settings for this context.
    /// </param>
    public SqliteDataContext(DbContextOptions<SqliteDataContext> options)
        : base(options)
    {
    }
}
