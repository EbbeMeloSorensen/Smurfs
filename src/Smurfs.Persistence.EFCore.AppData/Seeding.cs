using Smurfs.Domain;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Provides sample data for the app-data database.
/// </summary>
public class Seeding
{
    /// <summary>
    /// Saves the sample Smurfs when the Smurfs table is empty.
    /// </summary>
    public static async Task SeedDatabase(
        DataContext context)
    {
        if (context.Smurfs.Any())
            return;

        CreateDataForSeeding(
            out var smurfs);

        await context.Smurfs.AddRangeAsync(smurfs);

        context.SaveChanges();
    }

    private static void CreateDataForSeeding(
        out List<Smurf> smurfs)
    {
        smurfs = new List<Smurf>
        {
            new Smurf{ Name = "Papa Smurf"},
            new Smurf{ Name = "Smurfette"},
            new Smurf{ Name = "Clumsy Smurf"},
            new Smurf{ Name = "Grouchy Smurf"},
            new Smurf{ Name = "Brainy Smurf"},
            new Smurf{ Name = "Gutsy Smurf"},
            new Smurf{ Name = "Vanity Smurf"}
        };
    }
}
