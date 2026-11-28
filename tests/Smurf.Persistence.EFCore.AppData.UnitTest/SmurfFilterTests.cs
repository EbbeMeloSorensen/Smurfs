using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Smurfs.Application.Smurfs;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies combined filters are applied before totals and paging.
/// </summary>
public sealed class SmurfFilterTests
{
    /// <summary>
    /// Checks literal name matching and every combination of nullable infection states.
    /// </summary>
    [Theory]
    [InlineData(null, null, 5)]
    [InlineData("  Pa  ", null, 3)]
    [InlineData("PAPA", null, 2)]
    [InlineData("%", null, 1)]
    [InlineData("missing", null, 0)]
    [InlineData(" ", "", 5)]
    [InlineData(null, "true", 2)]
    [InlineData(null, "false", 2)]
    [InlineData(null, "null", 1)]
    [InlineData(null, "true|false", 4)]
    [InlineData(null, "true|null", 3)]
    [InlineData(null, "false|null", 3)]
    [InlineData(null, "true|false|null", 5)]
    [InlineData("pa", "false|null", 2)]
    public async Task List_AppliesFiltersBeforeCountingAndPaging(string? name, string? infected, int expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SqliteDataContext>().UseSqlite(connection).Options;
        await using var context = new SqliteDataContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Smurfs.AddRange(
            new Smurfs.Domain.VillageLife.Smurf { Name = "Papa", Infected = true },
            new Smurfs.Domain.VillageLife.Smurf { Name = "PAPA two", Infected = false },
            new Smurfs.Domain.VillageLife.Smurf { Name = "Other", Infected = true },
            new Smurfs.Domain.VillageLife.Smurf { Name = "painter", Infected = null },
            new Smurfs.Domain.VillageLife.Smurf { Name = "Literal %", Infected = false });
        await context.SaveChangesAsync();
        var handler = new Smurfs.Application.Smurfs.List.Handler(new SmurfReadService(context));
        var query = new Smurfs.Application.Smurfs.List.Query
        {
            Params = new SmurfParams { Name = name, Infected = infected, PageNumber = 1, PageSize = 1 }
        };
        var first = await handler.Handle(query, CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Equal(expected, first.Value.TotalCount);
        Assert.Equal(Math.Min(1, expected), first.Value.Count);
        Assert.Equal(expected, first.Value.TotalPages);
        query.Params.PageNumber = expected + 1;
        var beyond = await handler.Handle(query, CancellationToken.None);
        Assert.True(beyond.IsSuccess);
        Assert.Empty(beyond.Value);
        Assert.Equal(expected, beyond.Value.TotalCount);

        query.Params.Infected = "yes";
        Assert.False((await handler.Handle(query, CancellationToken.None)).IsSuccess);
    }
}
