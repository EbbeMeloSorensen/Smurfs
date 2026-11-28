using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Smurfs.Application.Smurfs;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies required UTC creation times and stable database sorting.
/// </summary>
public sealed class SmurfSortingTests
{
    /// <summary>
    /// Applies either sort before paging and uses identifiers for equal creation times.
    /// </summary>
    [Fact]
    public async Task List_SortsFilteredRowsBeforePagingAndPreservesUtc()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new SqliteDataContext(
            new DbContextOptionsBuilder<SqliteDataContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        var baseline = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var oldest = new Smurfs.Domain.VillageLife.Smurf { Name = "A", CreatedUtc = baseline, Infected = true };
        var newer = new Smurfs.Domain.VillageLife.Smurf { Name = "Z", CreatedUtc = baseline.AddDays(1), Infected = true };
        var tied = new Smurfs.Domain.VillageLife.Smurf { Name = "M", CreatedUtc = baseline.AddDays(1), Infected = true };
        context.Smurfs.AddRange(oldest, newer, tied,
            new Smurfs.Domain.VillageLife.Smurf { Name = "Excluded", CreatedUtc = baseline.AddDays(2), Infected = false });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var persisted = await context.Smurfs.SingleAsync(smurf => smurf.Id == oldest.Id);
        Assert.Equal(DateTimeKind.Utc, persisted.CreatedUtc.Kind);
        Assert.Equal(baseline, persisted.CreatedUtc);
        var handler = new Smurfs.Application.Smurfs.List.Handler(new SmurfReadService(context));
        var query = new Smurfs.Application.Smurfs.List.Query
        {
            Params = new SmurfParams { PageSize = 1, Infected = "true", Sorting = "created" }
        };
        var expected = new[] { newer, tied }.OrderBy(smurf => smurf.Id).Append(oldest).Select(smurf => smurf.Id).ToArray();
        var actual = new System.Collections.Generic.List<Guid>();
        for (var pageNumber = 1; pageNumber <= 3; pageNumber++)
        {
            query.Params.PageNumber = pageNumber;
            var result = await handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.TotalCount);
            actual.Add(Assert.Single(result.Value).Id);
        }

        Assert.Equal(expected, actual);
        query.Params.Sorting = "name";
        query.Params.PageNumber = 1;
        var alphabetical = await handler.Handle(query, CancellationToken.None);
        Assert.Equal(oldest.Id, Assert.Single(alphabetical.Value!).Id);
        query.Params.Sorting = "unsupported";
        Assert.False((await handler.Handle(query, CancellationToken.None)).IsSuccess);
    }

    /// <summary>
    /// Gives seed rows repeatable, distinct UTC timestamps and preserves them on reseeding.
    /// </summary>
    [Fact]
    public async Task Seeding_UsesFixedUtcCreationTimes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new SqliteDataContext(
            new DbContextOptionsBuilder<SqliteDataContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        await Seeding.SeedDatabase(context);
        var timestamps = await context.Smurfs.OrderBy(smurf => smurf.CreatedUtc).Select(smurf => smurf.CreatedUtc).ToListAsync();
        Assert.NotEmpty(timestamps);
        var baseline = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(Enumerable.Range(0, timestamps.Count).Select(index => baseline.AddMinutes(index)), timestamps);
        Assert.All(timestamps, value => Assert.Equal(DateTimeKind.Utc, value.Kind));
        await Seeding.SeedDatabase(context);
        Assert.Equal(timestamps, await context.Smurfs.OrderBy(smurf => smurf.CreatedUtc).Select(smurf => smurf.CreatedUtc).ToListAsync());
    }
}
