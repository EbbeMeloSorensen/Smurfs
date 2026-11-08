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
        services.AddDbContextFactory<DataContext>(options => options.UseSqlite(connection));
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

        var validName = new string('a', 20);
        var created = await handler.Handle(new Create.Command { Name = validName }, CancellationToken.None);
        Assert.True(created.IsSuccess);
        var saved = Assert.Single(await context.Smurfs.ToListAsync());
        Assert.Equal(validName, saved.Name);
        Assert.Equal(0, saved.Age);
    }

    private sealed class ReservedNameRule : IBusinessRule<SmurfNameDraft>
    {
        public BusinessRuleViolation? Validate(SmurfNameDraft draft) =>
            draft.Name == "Reserved" ? new(nameof(SmurfNameDraft.Name), "Name is reserved") : null;
    }
}
