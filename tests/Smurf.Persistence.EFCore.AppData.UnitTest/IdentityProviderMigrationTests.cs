using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Persistence.EFCore.Identity;
using CatalogSqliteContext = Smurfs.Persistence.EFCore.AppData.SqliteDataContext;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies Identity provider migration isolation and the shared context used by Identity stores.
/// </summary>
public sealed class IdentityProviderMigrationTests
{
    /// <summary>
    /// Verifies the deployed PostgreSQL migration ID and matching model snapshot without opening a connection.
    /// </summary>
    [Fact]
    public void Postgres_UsesExistingIdentityMigrationAndMatchingSnapshot()
    {
        using var context = new PostgresDataContextFactory().CreateDbContext([]);
        Assert.Equal("20261116175127_InitialMigration", Assert.Single(context.Database.GetMigrations()));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    /// <summary>
    /// Verifies SQLite uses an independent Identity migration and matching model snapshot.
    /// </summary>
    [Fact]
    public void Sqlite_UsesIndependentIdentityMigrationAndMatchingSnapshot()
    {
        using var context = new SqliteDataContextFactory().CreateDbContext([]);
        Assert.EndsWith("_InitialSqliteIdentity", Assert.Single(context.Database.GetMigrations()));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    /// <summary>
    /// Verifies repeatable Identity and catalog migration in one database and account operations through the shared context.
    /// </summary>
    [Fact]
    public async Task Sqlite_SharedRegistrationSupportsIdentityStoresAndCatalogMigration()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityPersistence<SqliteDataContext>(options => options.UseSqlite(connection));
        services.AddIdentityCore<AppUser>().AddEntityFrameworkStores<DataContext>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        Assert.Same(scope.ServiceProvider.GetRequiredService<SqliteDataContext>(), context);
        await context.Database.MigrateAsync();

        var catalogOptions = new DbContextOptionsBuilder<CatalogSqliteContext>().UseSqlite(connection).Options;
        await using var catalog = new CatalogSqliteContext(catalogOptions);
        await catalog.Database.MigrateAsync();
        await context.Database.MigrateAsync();
        await catalog.Database.MigrateAsync();

        var manager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var account = new AppUser
        {
            UserName = "migration-test",
            Email = "migration-test@example.test",
            DisplayName = "Migration Test"
        };
        var result = await manager.CreateAsync(account, "Pa$$w0rd");
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Description)));
        var saved = await manager.FindByNameAsync(account.UserName);
        Assert.NotNull(saved);
        Assert.True(await manager.CheckPasswordAsync(saved, "Pa$$w0rd"));
        Assert.Equal(2, (await context.Database.GetAppliedMigrationsAsync()).Count());
    }
}
