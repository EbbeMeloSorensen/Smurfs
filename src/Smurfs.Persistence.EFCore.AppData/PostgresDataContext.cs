using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Provides the shared catalog model with the Postgres migration history.
/// </summary>
public sealed class PostgresDataContext : DataContext
{
    /// <summary>
    /// Creates a context using the caller's Postgres provider options.
    /// </summary>
    /// <param name="options">
    /// The provider and connection settings for this context.
    /// </param>
    public PostgresDataContext(DbContextOptions<PostgresDataContext> options)
        : base(options)
    {
    }
}
