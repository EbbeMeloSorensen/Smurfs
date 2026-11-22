namespace Smurfs.Persistence.File;

/// <summary>
/// Creates or validates the versioned file store and seeds only an empty catalog.
/// </summary>
public sealed class AppDataInitializer(string filePath) : IAppDataInitializer
{
    /// <inheritdoc />
    public Task MigrateAsync() => new FileStore(filePath).InitializeAsync();

    /// <inheritdoc />
    public async Task SeedAsync()
    {
        var factory = new UnitOfWorkFactory(filePath);
        for (var attempt = 0; ; attempt++)
        {
            await using var session = factory.CreateUnitOfWork();
            if (await session.Smurfs.CountAsync([]) != 0)
            {
                return;
            }

            foreach (var smurf in SampleSmurfs.Create())
            {
                await session.Smurfs.AddAsync(smurf);
            }

            try
            {
                await session.CompleteAsync();
                return;
            }
            catch (FileConflictException) when (attempt < 2)
            {
                // Another initializer or writer committed. Recheck the latest catalog.
            }
        }
    }
}
