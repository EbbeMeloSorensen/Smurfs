namespace Smurfs.Persistence.EFCore.AppData;

public class Seeding
{
    public static async Task SeedDatabase(
        DataContext context)
    {
        if (context.Smurfs.Any())
            return;

        //CreateDataForSeeding(
        //    PRDbContextBase.Versioned,
        //    out var people,
        //    out var personComments,
        //    out var smurfs);

        //await context.Smurfs.AddRangeAsync(smurfs);
        //await context.People.AddRangeAsync(people);
        //await context.PersonComments.AddRangeAsync(personComments);

        context.SaveChanges();
    }
}
