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
/// Verifies that the create command uses the domain catalog before writing to SQLite.
/// </summary>
public sealed class CreateSmurfTests
{
    /// <summary>
    /// Verifies that invalid names and an additional catalog rule block writes while a valid name is persisted.
    /// </summary>
    [Fact]
    public async Task Create_UsesCatalogForDraftAndPersistsOnlyValidSmurfs()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddAppDataPersistence<SqliteDataContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<DataContext>>();
        await using var context = await factory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();

        var catalog = new BusinessRuleCatalog();
        catalog.RegisterAtomicRule(new ReservedNameRule());
        var handler = new Create.Handler(new UnitOfWorkFactory(factory), catalog);

        foreach (var name in new[] { "   ", new string('a', 21), "Reserved" })
        {
            var expected = Assert.Single(catalog.ValidateAtomic(new SmurfNameDraft(name)));
            var result = await handler.Handle(new Create.Command { Name = name }, CancellationToken.None);
            Assert.False(result.IsSuccess);
            Assert.Equal(expected.Message, result.Error);
        }

        Assert.Empty(await context.Smurfs.ToListAsync());

        var beforeCreate = DateTime.UtcNow;
        var validName = new string('a', 20);
        var created = await handler.Handle(new Create.Command { Name = validName }, CancellationToken.None);
        Assert.True(created.IsSuccess);
        var saved = Assert.Single(await context.Smurfs.ToListAsync());
        Assert.Equal(validName, saved.Name);
        Assert.Equal(0, saved.Health);
        Assert.InRange(saved.CreatedUtc, beforeCreate, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, saved.CreatedUtc.Kind);
        var birthday = new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var detailed = await handler.Handle(new Create.Command
        {
            Name = "Detailed",
            Health = 42,
            Birthday = birthday,
            Infected = false,
            Gender = Gender.Female
        }, CancellationToken.None);
        Assert.True(detailed.IsSuccess);
        context.ChangeTracker.Clear();
        var detailedSmurf = await context.Smurfs.SingleAsync(smurf => smurf.Name == "Detailed");
        Assert.Equal(42, detailedSmurf.Health);
        Assert.Equal(birthday, detailedSmurf.Birthday);
        Assert.Equal(false, detailedSmurf.Infected);
        Assert.Equal(Gender.Female, detailedSmurf.Gender);

    }

    private sealed class ReservedNameRule : IBusinessRule<SmurfNameDraft>
    {
        public BusinessRuleViolation? Validate(SmurfNameDraft draft) =>
            draft.Name == "Reserved" ? new(nameof(SmurfNameDraft.Name), "Name is reserved") : null;
    }
}
