using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.Persistence.EFCore.AppData.Repositories;
using SmurfEntity = Smurfs.Domain.Smurf;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies relational repository and session behavior against isolated in-memory SQLite databases.
/// </summary>
public sealed class PersistenceBaselineTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    private DbContextOptions<DataContext> Options => new DbContextOptionsBuilder<DataContext>()
        .UseSqlite(_connection).Options;

    private static readonly Expression<Func<SmurfEntity, bool>>[] _all = [];

    /// <summary>
    /// Opens an isolated database and creates the configured schema.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var context = new DataContext(Options);
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Closes the connection and releases the in-memory database.
    /// </summary>
    public Task DisposeAsync() => _connection.DisposeAsync().AsTask();

    /// <summary>
    /// Verifies filter conjunction, unfiltered queries and empty results.
    /// </summary>
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
        Assert.Equal(3, await repository.CountAsync(_all));
        Assert.Equal(3, (await repository.FindAsync(_all)).Count);
        Assert.Empty(await repository.FindAsync([s => s.Age < 0]));
    }

    /// <summary>
    /// Verifies that default read tracking allows edits to persist without an explicit Update.
    /// </summary>
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
            var entity = Assert.Single(await new SmurfRepository(context).FindAsync(_all));
            Assert.Equal(EntityState.Unchanged, context.Entry(entity).State);
            entity.Name = "After";
            await context.SaveChangesAsync();
        }
        await using var verify = new DataContext(Options);
        Assert.Equal("After", (await verify.Smurfs.SingleAsync()).Name);
    }

    /// <summary>
    /// Verifies detached updates and the Unit of Work commit boundary.
    /// </summary>
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
            {
                Assert.Equal("Before", (await beforeCommit.Smurfs.SingleAsync()).Name);
            }

            await unit.CompleteAsync();
        }
        await using var verify = new DataContext(Options);
        Assert.Equal("After", (await verify.Smurfs.SingleAsync()).Name);
    }

    /// <summary>
    /// Verifies insert and delete staging against independent read contexts.
    /// </summary>
    [Fact]
    public async Task AddAndRemove_DoNotSaveUntilComplete()
    {
        var entity = new SmurfEntity { Name = "Papa" };
        await using var unit = new UnitOfWork(new DataContext(Options));
        await unit.Smurfs.AddAsync(entity);
        await using (var beforeCommit = new DataContext(Options))
        {
            Assert.Empty(await beforeCommit.Smurfs.ToListAsync());
        }

        await unit.CompleteAsync();
        await unit.Smurfs.RemoveAsync(entity);
        await using (var beforeDelete = new DataContext(Options))
        {
            Assert.Single(await beforeDelete.Smurfs.ToListAsync());
        }

        await unit.CompleteAsync();
        await using var verify = new DataContext(Options);
        Assert.Empty(await verify.Smurfs.ToListAsync());
    }

    /// <summary>
    /// Verifies context ownership and absence of an implicit commit on disposal.
    /// </summary>
    [Fact]
    public async Task Disposal_DiscardsUncommittedInsert_AndDisposesOwnedContext()
    {
        var context = new DataContext(Options);
        await using (var unit = new UnitOfWork(context))
        {
            await unit.Smurfs.AddAsync(new SmurfEntity { Name = "Unsaved" });
        }

        Assert.Throws<ObjectDisposedException>(() => context.Smurfs.ToList());
        await using var verify = new DataContext(Options);
        Assert.Empty(await verify.Smurfs.ToListAsync());
    }

    /// <summary>
    /// Verifies that already-cancelled mutations leave the change tracker empty.
    /// </summary>
    /// <param name="operation">
    /// The staging operation to cancel.
    /// </param>
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

    /// <summary>
    /// Verifies that current mappings match the regenerated migration snapshot.
    /// </summary>
    [Fact]
    public async Task Model_IncludesVillageAndPets_AndMatchesMigrationSnapshot()
    {
        await using var context = new DataContext(Options);
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Equal(3, context.Model.GetEntityTypes().Count());
        Assert.Equal("Pets", context.Model.FindEntityType(typeof(Smurfs.Domain.Pet))!.GetTableName());
        Assert.Equal("Villages", context.Model.FindEntityType(typeof(Smurfs.Domain.Village))!.GetTableName());
        var relationships = context.Model.FindEntityType(typeof(SmurfEntity))!.GetForeignKeys().ToArray();
        Assert.Equal(2, relationships.Length);
        Assert.All(relationships, relationship =>
        {
            Assert.False(relationship.IsRequired);
            Assert.False(relationship.IsUnique);

        });
        var village = Assert.Single(relationships, relationship =>
            relationship.PrincipalEntityType.ClrType == typeof(Smurfs.Domain.Village));
        Assert.Equal(DeleteBehavior.ClientSetNull, village.DeleteBehavior);
        var pet = Assert.Single(relationships, relationship =>
            relationship.PrincipalEntityType.ClrType == typeof(Smurfs.Domain.Pet));
        Assert.Equal(DeleteBehavior.Restrict, pet.DeleteBehavior);
        Assert.Equal(nameof(SmurfEntity.PetId), Assert.Single(pet.Properties).Name);
        var foreignKey = Assert.Single(village.Properties);
        Assert.Equal("VillageId", foreignKey.Name);
        Assert.True(foreignKey.IsShadowProperty());
        Assert.Equal(typeof(Guid?), foreignKey.ClrType);
    }

    /// <summary>
    /// Verifies village membership and shared pets survive a relational round trip.
    /// </summary>
    [Fact]
    public async Task VillageMembership_AndSharedPet_RoundTrip()
    {
        await using (var context = new DataContext(Options))
        {
            var pet = new Smurfs.Domain.Pet { Name = "Shared" };
            var village = new Smurfs.Domain.Village { Name = "Home" };
            village.Smurfs.Add(new SmurfEntity { Name = "Papa", Pet = pet });
            village.Smurfs.Add(new SmurfEntity { Name = "Brainy", Pet = pet });
            context.Villages.Add(village);
            context.Smurfs.Add(new SmurfEntity { Name = "Traveller" });
            await context.SaveChangesAsync();
        }

        await using var verify = new DataContext(Options);
        var saved = await verify.Villages.Include(v => v.Smurfs).ThenInclude(s => s.Pet).SingleAsync();
        Assert.Equal(2, saved.Smurfs.Count);
        var petId = (await verify.Pets.SingleAsync()).Id;
        Assert.All(saved.Smurfs, smurf =>
        {
            Assert.Equal(petId, smurf.PetId);
            Assert.Equal(saved.Id, verify.Entry(smurf).Property<Guid?>("VillageId").CurrentValue);
        });
        var traveller = await verify.Smurfs.SingleAsync(s => s.Name == "Traveller");
        Assert.Null(verify.Entry(traveller).Property<Guid?>("VillageId").CurrentValue);
    }

    /// <summary>
    /// Verifies Restrict blocks referenced Pet deletion unless EF clears tracked optional relationships.
    /// </summary>
    /// <param name="trackSmurfs">
    /// Whether the referencing Smurfs are loaded into the deleting context.
    /// </param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PetDeletion_RespectsTrackedAndUntrackedReferences(bool trackSmurfs)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;
        await using (var seed = new DataContext(options))
        {
            await seed.Database.MigrateAsync();
            var pet = new Smurfs.Domain.Pet { Name = "Shared" };
            seed.Smurfs.AddRange(new SmurfEntity { Name = "Papa", Pet = pet },
                new SmurfEntity { Name = "Brainy", Pet = pet });
            await seed.SaveChangesAsync();
        }

        await using (var context = new DataContext(options))
        {
            var pet = await context.Pets.SingleAsync();
            if (trackSmurfs)
            {
                await context.Smurfs.LoadAsync();
            }

            context.Pets.Remove(pet);
            if (trackSmurfs)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                var error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                var sqliteError = Assert.IsType<SqliteException>(error.InnerException);
                Assert.Equal(19, sqliteError.SqliteErrorCode);
            }
        }

        await using var verify = new DataContext(options);
        Assert.Equal(2, await verify.Smurfs.CountAsync());
        Assert.Equal(trackSmurfs ? 0 : 1, await verify.Pets.CountAsync());
        var smurfs = await verify.Smurfs.ToListAsync();
        Assert.All(smurfs, smurf => Assert.Equal(!trackSmurfs, smurf.PetId.HasValue));
    }

    /// <summary>
    /// Verifies that the initial migration creates usable village and pet relationships.
    /// </summary>
    [Fact]
    public async Task InitialMigration_CreatesUsableSchemaInFreshDatabase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;
        await using var context = new DataContext(options);
        await context.Database.MigrateAsync();
        Assert.Equal(context.Database.GetMigrations(), await context.Database.GetAppliedMigrationsAsync());
        var pet = new Smurfs.Domain.Pet { Name = "Shared" };
        var village = new Smurfs.Domain.Village { Name = "Migrated" };
        village.Smurfs.Add(new SmurfEntity { Name = "Papa", Pet = pet });
        village.Smurfs.Add(new SmurfEntity { Name = "Brainy", Pet = pet });
        context.Villages.Add(village);
        context.Smurfs.Add(new SmurfEntity { Name = "Traveller" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var saved = await context.Villages.Include(v => v.Smurfs).ThenInclude(s => s.Pet).SingleAsync();
        Assert.Equal(2, saved.Smurfs.Count);
        Assert.Single(await context.Pets.ToListAsync());
        Assert.All(saved.Smurfs, smurf =>
        {
            Assert.Equal(pet.Id, smurf.PetId);
            Assert.NotNull(smurf.Pet);
            Assert.Equal(saved.Id, context.Entry(smurf).Property<Guid?>("VillageId").CurrentValue);
        });
        var traveller = await context.Smurfs.SingleAsync(s => s.Name == "Traveller");
        Assert.Null(context.Entry(traveller).Property<Guid?>("VillageId").CurrentValue);
    }
}
