namespace Smurfs.Persistence;

public interface IAppDataInitializer
{
    Task MigrateAsync();

    Task SeedAsync();
}
