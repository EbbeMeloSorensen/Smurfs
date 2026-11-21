using Craft.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Application.Smurfs;
using Smurfs.Domain;
using Smurfs.Domain.BusinessRules;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies persisted edits and rejection without partial writes.
/// </summary>
public sealed class EditSmurfTests
{
    /// <summary>
    /// Preserves relationships and the identifier while rejecting invalid and missing edits.
    /// </summary>
    [Fact]
    public async Task Edit_PersistsValidChangesAndRejectsInvalidOrMissingSmurfs()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddAppDataPersistence<SqliteDataContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<DataContext>>();
        await using var context = await factory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        var pet = new Pet { Name = "Pet" };
        var original = new Smurfs.Domain.Smurf { Name = "Original", Age = 7, Pet = pet };
        context.Smurfs.Add(original);
        await context.SaveChangesAsync();
        var petId = original.PetId;
        var catalog = new BusinessRuleCatalog();
        catalog.RegisterAtomicRule(new RejectedAgeRule());
        var handler = new Edit.Handler(new UnitOfWorkFactory(factory), catalog);

        foreach (var name in new[] { " ", new string('a', 21) })
        {
            var rejected = await handler.Handle(
                new Edit.Command { Id = original.Id, Name = name, Age = 12 }, CancellationToken.None);
            Assert.False(rejected.IsSuccess);
        }

        var entityRejected = await handler.Handle(
            new Edit.Command { Id = original.Id, Name = "Changed", Age = 99 }, CancellationToken.None);
        Assert.False(entityRejected.IsSuccess);
        await context.Entry(original).ReloadAsync();
        Assert.Equal("Original", original.Name);
        Assert.Equal(7, original.Age);

        var missing = await handler.Handle(
            new Edit.Command { Id = Guid.NewGuid(), Name = "Missing", Age = 1 }, CancellationToken.None);
        Assert.False(missing.IsSuccess);
        Assert.Equal("Smurf not found.", missing.Error);

        var saved = await handler.Handle(
            new Edit.Command { Id = original.Id, Name = "Edited", Age = 12 }, CancellationToken.None);
        Assert.True(saved.IsSuccess);
        await context.Entry(original).ReloadAsync();
        Assert.Equal("Edited", original.Name);
        Assert.Equal(12, original.Age);
        Assert.Equal(petId, original.PetId);
        Assert.Single(await context.Smurfs.ToListAsync());
    }

    private sealed class RejectedAgeRule : IBusinessRule<Smurfs.Domain.Smurf>
    {
        public BusinessRuleViolation? Validate(Smurfs.Domain.Smurf smurf) =>
            smurf.Age == 99 ? new(nameof(Smurfs.Domain.Smurf.Age), "Test age rejected.") : null;
    }
}
