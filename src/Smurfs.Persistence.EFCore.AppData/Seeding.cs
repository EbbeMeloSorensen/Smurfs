namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Provides sample data for the app-data database.
/// </summary>
public class Seeding
{
    /// <summary>
    /// Saves seven named samples and the configured number of randomly named Smurfs when the Smurfs table is empty.
    /// </summary>
    public static async Task SeedDatabase(
        DataContext context)
    {
        if (context.Smurfs.Any())
        {
            return;
        }

        var smurfs = SampleSmurfs.Create();

        await context.Smurfs.AddRangeAsync(smurfs);

        context.SaveChanges();
    }
}
