using Smurfs.Domain;

namespace Smurfs.Persistence;

/// <summary>
/// Creates the shared demo catalog for application persistence providers.
/// </summary>
public static class SampleSmurfs
{
    /// <summary>
    /// Returns seven named samples and one random name, with fixed UTC creation times.
    /// </summary>
    public static IReadOnlyList<Smurf> Create()
    {
        const int randomSmurfCount = 1;
        var seedCreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var smurfs = new List<Smurf>
        {
            new Smurf{ Name = "Papa Smurf", CreatedUtc = seedCreatedUtc.AddMinutes(0) },
            new Smurf{ Name = "Smurfette", CreatedUtc = seedCreatedUtc.AddMinutes(1) },
            new Smurf{ Name = "Clumsy Smurf", CreatedUtc = seedCreatedUtc.AddMinutes(2) },
            new Smurf{ Name = "Grouchy Smurf", CreatedUtc = seedCreatedUtc.AddMinutes(3) },
            new Smurf{ Name = "Brainy Smurf", CreatedUtc = seedCreatedUtc.AddMinutes(4) },
            new Smurf{ Name = "Gutsy Smurf", CreatedUtc = seedCreatedUtc.AddMinutes(5) },
            new Smurf{ Name = "Vanity Smurf", CreatedUtc = seedCreatedUtc.AddMinutes(6) }
        };

        string[] adjectives = ["Bashful", "Bright", "Cheerful", "Curious", "Drowsy", "Jolly", "Merry", "Quiet"];
        string[] hobbies = ["Baker", "Dancer", "Gardener", "Painter", "Poet", "Singer", "Tailor", "Writer"];

        // Names can repeat so paging exercises its identifier tie-breaker.
        for (var index = 0; index < randomSmurfCount; index++)
        {
            var adjective = adjectives[Random.Shared.Next(adjectives.Length)];
            var hobby = hobbies[Random.Shared.Next(hobbies.Length)];
            smurfs.Add(new Smurf { Name = $"{adjective} {hobby}", CreatedUtc = seedCreatedUtc.AddMinutes(smurfs.Count) });
        }

        return smurfs;
    }
}
