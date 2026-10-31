using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.Persistence.EFCore.AppData.Repositories;
using SmurfEntity = Smurfs.Domain.Smurf;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>Verifies relational repository and session behavior against isolated in-memory SQLite databases.</summary>
public sealed class PersistenceBaselineTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private DbContextOptions<DataContext> Options => new DbContextOptionsBuilder<DataContext>()
        .UseSqlite(_connection).Options;
    private static readonly Expression<Func<SmurfEntity, bool>>[] All = [];

    /// <summary>Opens an isolated database and creates the convention-based schema.</summary>
    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var context = new DataContext(Options);
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>Closes the connection and releases the in-memory database.</summary>
    public Task DisposeAsync() => _connection.DisposeAsync().AsTask();

    /// <summary>Verifies filter conjunction, unfiltered queries and empty results.</summary>
    [Fact]
    public async Task Queries_CombinePredicatesWithAnd_AndAcceptEmptyFilters()
    {
        await using var context = new DataContext(Options);
        context.AddRange(new SmurfEntity { Name = "Papa", Age = 100 },
            new SmurfEntity { Name = "Papa", Age = 20 },
            new SmurfEntity { Name = "Other", Age = 100 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = new SmurfRepository(context);
        Expression<Func<SmurfEntity, bool>>[] filters = [s => s.Name == "Papa", s => s.Age > 50];

        Assert.Single(await repository.FindAsync(filters));
        Assert.Equal(1, await repository.CountAsync(filters));
        Assert.Equal(3, await repository.CountAsync(All));
        Assert.Equal(3, (await repository.FindAsync(All)).Count);
        Assert.Empty(await repository.FindAsync([s => s.Age < 0]));
    }

    /// <summary>Verifies that default read tracking allows edits to persist without an explicit Update.</summary>
    [Fact]
    public async Task Find_TracksByDefault_AndTrackedEditsAreCommitted()
    {
        await using (var seed = new DataContext(Options))
        {
            seed.Add(new SmurfEntity { Name = "Before" });
            await seed.SaveChangesAsync();
        }
        await using (var context = new DataContext(Options))
        {
            var entity = Assert.Single(await new SmurfRepository(context).FindAsync(All));
            Assert.Equal(EntityState.Unchanged, context.Entry(entity).State);
            entity.Name = "After";
            await context.SaveChangesAsync();
        }
        await using var verify = new DataContext(Options);
        Assert.Equal("After", (await verify.Smurfs.SingleAsync()).Name);
    }

    /// <summary>Verifies detached updates and the Unit of Work commit boundary.</summary>
    [Fact]
    public async Task DetachedUpdate_IsStagedUntilComplete_AndPersistsThroughUnitOfWork()
    {
        SmurfEntity entity;
        await using (var seed = new DataContext(Options))
        {
            entity = new SmurfEntity { Name = "Before" };
            seed.Add(entity);
            await seed.SaveChangesAsync();
        }
        entity.Name = "After";
        await using (var unit = new UnitOfWork(new DataContext(Options)))
        {
            await unit.Smurfs.UpdateAsync(entity);
            await using (var beforeCommit = new DataContext(Options))
                Assert.Equal("Before", (await beforeCommit.Smurfs.SingleAsync()).Name);
            await unit.CompleteAsync();
        }
        await using var verify = new DataContext(Options);
        Assert.Equal("After", (await verify.Smurfs.SingleAsync()).Name);
    }

    /// <summary>Verifies insert and delete staging against independent read contexts.</summary>
    [Fact]
    public async Task AddAndRemove_DoNotSaveUntilComplete()
    {
        var entity = new SmurfEntity { Name = "Papa" };
        await using var unit = new UnitOfWork(new DataContext(Options));
        await unit.Smurfs.AddAsync(entity);
        await using (var beforeCommit = new DataContext(Options))
            Assert.Empty(await beforeCommit.Smurfs.ToListAsync());
        await unit.CompleteAsync();
        await unit.Smurfs.RemoveAsync(entity);
        await using (var beforeDelete = new DataContext(Options))
            Assert.Single(await beforeDelete.Smurfs.ToListAsync());
        await unit.CompleteAsync();
        await using var verify = new DataContext(Options);
        Assert.Empty(await verify.Smurfs.ToListAsync());
    }

    /// <summary>Verifies context ownership and absence of an implicit commit on disposal.</summary>
    [Fact]
    public async Task Disposal_DiscardsUncommittedInsert_AndDisposesOwnedContext()
    {
        var context = new DataContext(Options);
        await using (var unit = new UnitOfWork(context))
            await unit.Smurfs.AddAsync(new SmurfEntity { Name = "Unsaved" });
        Assert.Throws<ObjectDisposedException>(() => context.Smurfs.ToList());
        await using var verify = new DataContext(Options);
        Assert.Empty(await verify.Smurfs.ToListAsync());
    }

    /// <summary>Verifies that already-cancelled mutations leave the change tracker empty.</summary>
    /// <param name="operation">The staging operation to cancel.</param>
    [Theory]
    [InlineData("Add")]
    [InlineData("Update")]
    [InlineData("Remove")]
    public async Task CancelledMutation_DoesNotAttachEntity(string operation)
    {
        await using var context = new DataContext(Options);
        var repository = new SmurfRepository(context);
        var entity = new SmurfEntity { Name = "Cancelled" };
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation switch
        {
            "Add" => repository.AddAsync(entity, cancellation.Token),
            "Update" => repository.UpdateAsync(entity, cancellation.Token),
            _ => repository.RemoveAsync(entity, cancellation.Token)
        });
        Assert.Empty(context.ChangeTracker.Entries());
    }

    /// <summary>Verifies migration consistency and optional shared-pet cardinality.</summary>
    [Fact]
    public async Task Model_MatchesMigrationSnapshot_AndIncludesOnlySmurfAndPet()
    {
        await using var context = new DataContext(Options);
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Equal(2, context.Model.GetEntityTypes().Count());
        var relationship = Assert.Single(context.Model.FindEntityType(typeof(SmurfEntity))!.GetForeignKeys());
        Assert.False(relationship.IsRequired);
        Assert.False(relationship.IsUnique);
        Assert.Equal(typeof(Smurfs.Domain.Pet), relationship.PrincipalEntityType.ClrType);
    }

    /// <summary>Verifies that the checked-in migration creates a usable relational schema.</summary>
    [Fact]
    public async Task InitialMigration_CreatesUsableSchemaInFreshDatabase()
    {
        // Use a second database because the fixture schema was created via EnsureCreated.
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;
        await using (var context = new DataContext(options))
        {
            await context.Database.MigrateAsync();
            context.Add(new SmurfEntity { Name = "Migrated", Pet = new Smurfs.Domain.Pet { Name = "Shared" } });
            await context.SaveChangesAsync();
        }
        await using var verify = new DataContext(options);
        Assert.Equal("Shared", (await verify.Smurfs.Include(s => s.Pet).SingleAsync()).Pet!.Name);
    }
}
