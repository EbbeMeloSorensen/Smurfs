using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies that provider contexts discover independent migration histories in the shared assembly.
/// </summary>
public sealed class ProviderMigrationTests
{
    /// <summary>
    /// Verifies that PostgreSQL retains its deployed migration identity and matches the current model.
    /// </summary>
    [Fact]
    public void Postgres_UsesExistingMigrationAndMatchingSnapshot()
    {
        using var context = new PostgresDataContextFactory().CreateDbContext([]);
        var migrations = context.Database.GetMigrations().ToArray();
        Assert.Contains("20261118194640_InitialPostgres", migrations);
        Assert.Contains("20261122081738_AddedCreatedTimestampToSmurf", migrations);
        Assert.All(migrations, migration => Assert.DoesNotContain("Sqlite", migration));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    /// <summary>
    /// Verifies scoped context resolution, asynchronous factory creation, and repeatable initialization.
    /// </summary>
    [Fact]
    public async Task Sqlite_RegistrationUsesProviderForSharedServices()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddAppDataPersistence<SqliteDataContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        Assert.Same(scope.ServiceProvider.GetRequiredService<SqliteDataContext>(), context);

        var initializer = scope.ServiceProvider.GetRequiredService<IAppDataInitializer>();
        await initializer.MigrateAsync();
        await initializer.SeedAsync();
        var count = await context.Smurfs.CountAsync();
        Assert.Equal(8, count);
        await initializer.MigrateAsync();
        await initializer.SeedAsync();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>();
        await using var independentContext = await factory.CreateDbContextAsync();
        Assert.IsType<SqliteDataContext>(independentContext);
        Assert.NotSame(context, independentContext);
        Assert.Equal(count, await independentContext.Smurfs.CountAsync());
    }

    /// <summary>
    /// Verifies that SQLite discovers its own baseline and matching snapshot.
    /// </summary>
    [Fact]
    public void Sqlite_UsesIndependentMigrationAndMatchingSnapshot()
    {
        using var context = new SqliteDataContextFactory().CreateDbContext([]);
        var migrations = context.Database.GetMigrations().ToArray();
        Assert.Contains("20261118192145_InitialSqlite", migrations);
        Assert.Contains("20261122081748_AddedCreatedTimestampToSmurf", migrations);
        Assert.All(migrations, migration => Assert.DoesNotContain("Postgres", migration));
        Assert.False(context.Database.HasPendingModelChanges());
    }
}
