using Smurfs.Domain;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Provides sample data for the app-data database.
/// </summary>
public class Seeding
{
    /// <summary>
    /// Saves the named samples and 1,000 randomly named Smurfs when the Smurfs table is empty.
    /// </summary>
    public static async Task SeedDatabase(
        DataContext context)
    {
        if (context.Smurfs.Any())
        {
            return;
        }

        CreateDataForSeeding(
            1,
            out var smurfs);

        await context.Smurfs.AddRangeAsync(smurfs);

        context.SaveChanges();
    }

    private static void CreateDataForSeeding(
        int randomSmurfCount,
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

        //const int randomSmurfCount = 1_000;
        string[] adjectives = ["Bashful", "Bright", "Cheerful", "Curious", "Drowsy", "Jolly", "Merry", "Quiet"];
        string[] hobbies = ["Baker", "Dancer", "Gardener", "Painter", "Poet", "Singer", "Tailor", "Writer"];

        // Names can repeat so paging exercises its identifier tie-breaker.
        for (var index = 0; index < randomSmurfCount; index++)
        {
            var adjective = adjectives[Random.Shared.Next(adjectives.Length)];
            var hobby = hobbies[Random.Shared.Next(hobbies.Length)];
            smurfs.Add(new Smurf { Name = $"{adjective} {hobby}" });
        }
    }
}
