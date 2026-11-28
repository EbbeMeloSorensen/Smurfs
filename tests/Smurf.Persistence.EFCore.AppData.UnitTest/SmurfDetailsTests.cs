using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Application.Smurfs;
using Smurfs.Domain.VillageLife;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies single-Smurf retrieval for direct detail navigation.
/// </summary>
public sealed class SmurfDetailsTests
{
    /// <summary>
    /// Returns persisted optional fields and reports a missing identifier.
    /// </summary>
    [Fact]
    public async Task Details_ReturnsFieldsAndRejectsMissingSmurf()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddAppDataPersistence<SqliteDataContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<DataContext>>();
        await using var context = await factory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        var smurf = new Smurfs.Domain.VillageLife.Smurf
        {
            Name = "Kylling",
            Health = 42,
            Birthday = new DateTime(2000, 1, 2),
            Infected = false,
            Gender = Gender.Female
        };
        context.Smurfs.Add(smurf);
        await context.SaveChangesAsync();
        var handler = new Details.Handler(new UnitOfWorkFactory(factory));
        var result = await handler.Handle(new Details.Query { Id = smurf.Id }, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(smurf.Id, result.Value.Id);
        Assert.Equal(smurf.Name, result.Value.Name);
        Assert.Equal(smurf.CreatedUtc, result.Value.CreatedUtc);
        Assert.Equal(DateTimeKind.Utc, result.Value.CreatedUtc.Kind);
        Assert.Equal(42, result.Value.Health);
        Assert.Equal(smurf.Birthday, result.Value.Birthday);
        Assert.Equal(false, result.Value.Infected);
        Assert.Equal(Gender.Female, result.Value.Gender);
        Assert.Null(result.Value.PetId);
        var missing = await handler.Handle(new Details.Query { Id = Guid.NewGuid() }, CancellationToken.None);
        Assert.False(missing.IsSuccess);
    }
}
