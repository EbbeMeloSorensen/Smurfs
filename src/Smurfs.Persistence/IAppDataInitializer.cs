namespace Smurfs.Persistence;

/// <summary>
/// Defines repeatable setup operations for application storage.
/// </summary>
public interface IAppDataInitializer
{
    /// <summary>
    /// Prepares storage and applies supported schema or file-format upgrades.
    /// </summary>
    Task MigrateAsync();

    /// <summary>
    /// Initializes sample data when prepared storage is empty.
    /// </summary>
    Task SeedAsync();
}
