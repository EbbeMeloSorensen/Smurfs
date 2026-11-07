namespace Smurfs.Persistence;

/// <summary>
/// Defines setup operations for the application database.
/// </summary>
public interface IAppDataInitializer
{
    /// <summary>
    /// Applies outstanding application database migrations.
    /// </summary>
    Task MigrateAsync();

    /// <summary>
    /// Initializes sample data in the migrated database.
    /// </summary>
    Task SeedAsync();
}
