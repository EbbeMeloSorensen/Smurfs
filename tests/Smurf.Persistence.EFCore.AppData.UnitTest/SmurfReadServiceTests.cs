using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Smurfs.Domain;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies that paged reads include populated and unknown optional fields.
/// </summary>
public sealed class SmurfReadServiceTests
{
    /// <summary>
    /// Projects scalar fields while preserving null and false as distinct values.
    /// </summary>
    [Fact]
    public async Task GetPage_ProjectsAllScalarFieldsAndNulls()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SqliteDataContext>().UseSqlite(connection).Options;
        await using var context = new SqliteDataContext(options);
        await context.Database.EnsureCreatedAsync();
        var pet = new Pet { Name = "Pet" };
        var known = new Smurfs.Domain.Smurf
        {
            Name = "A",
            Health = 42,
            Birthday = new DateTime(2000, 1, 2),
            Infected = false,
            Gender = Gender.Female,
            Pet = pet
        };
        var unknown = new Smurfs.Domain.Smurf { Name = "B" };
        context.Smurfs.AddRange(known, unknown);
        await context.SaveChangesAsync();

        var page = await new SmurfReadService(context).GetPageAsync(1, 25, CancellationToken.None);
        Assert.Equal(2, page.TotalCount);
        var first = page[0];
        Assert.Equal(known.Id, first.Id);
        Assert.Equal("A", first.Name);
        Assert.Equal(42, first.Health);
        Assert.Equal(known.Birthday, first.Birthday);
        Assert.Equal(false, first.Infected);
        Assert.Equal(Gender.Female, first.Gender);
        Assert.Equal(pet.Id, first.PetId);
        var second = page[1];
        Assert.Null(second.Birthday);
        Assert.Null(second.Infected);
        Assert.Null(second.Gender);
        Assert.Null(second.PetId);
    }
}
