using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Application.Smurfs;
using Smurfs.Domain;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies deletion through the application persistence boundary.
/// </summary>
public sealed class DeleteSmurfTests
{
    /// <summary>
    /// Deletes only the selected Smurf, preserves its pet, and rejects missing identifiers.
    /// </summary>
    [Fact]
    public async Task Delete_RemovesSelectedSmurfAndRejectsMissingIdentifier()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddAppDataPersistence<SqliteDataContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<DataContext>>();
        await using var context = await factory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        var pet = new Pet { Name = "Shared pet" };
        var selected = new Smurfs.Domain.Smurf { Name = "Selected", Pet = pet };
        var remaining = new Smurfs.Domain.Smurf { Name = "Remaining", Pet = pet };
        context.Smurfs.AddRange(selected, remaining);
        await context.SaveChangesAsync();
        var handler = new Delete.Handler(new UnitOfWorkFactory(factory));

        var deleted = await handler.Handle(new Delete.Command { Id = selected.Id }, CancellationToken.None);
        Assert.True(deleted.IsSuccess);
        context.ChangeTracker.Clear();
        Assert.Equal(remaining.Id, Assert.Single(await context.Smurfs.ToListAsync()).Id);
        Assert.Equal(pet.Id, Assert.Single(await context.Pets.ToListAsync()).Id);

        foreach (var id in new[] { selected.Id, Guid.NewGuid() })
        {
            var missing = await handler.Handle(new Delete.Command { Id = id }, CancellationToken.None);
            Assert.False(missing.IsSuccess);
            Assert.Equal("Smurf not found.", missing.Error);
        }

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.Handle(new Delete.Command { Id = remaining.Id }, cancellation.Token));
        Assert.Single(await context.Smurfs.ToListAsync());
    }
}
