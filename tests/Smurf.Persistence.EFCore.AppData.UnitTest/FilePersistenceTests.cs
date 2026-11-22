using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Application.Smurfs;
using Smurfs.Domain;
using Smurfs.Domain.BusinessRules;
using Smurfs.Persistence;
using Smurfs.Persistence.File;
using FileReadService = Smurfs.Persistence.File.Smurfs.SmurfReadService;
using SmurfEntity = Smurfs.Domain.Smurf;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Exercises the file adapter using isolated on-disk catalogs and independent sessions.
/// </summary>
public sealed class FilePersistenceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "smurfs-file-tests", Guid.NewGuid().ToString("N"));

    private string CatalogPath => Path.Combine(_directory, "catalog.json");

    /// <summary>
    /// Initialization can be repeated or raced without replacing existing data or duplicating seeds.
    /// </summary>
    [Fact]
    public async Task Initialization_SeedsOnceAndPreservesExistingRows()
    {
        var initializer = new AppDataInitializer(CatalogPath);
        await Task.WhenAll(Enumerable.Range(0, 4).Select(async _ =>
        {
            await initializer.MigrateAsync();
            await initializer.SeedAsync();
        }));
        var first = await ReadAll();
        Assert.Equal(8, first.Count);
        Assert.All(first, smurf => Assert.Equal(DateTimeKind.Utc, smurf.CreatedUtc.Kind));
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), first.Min(smurf => smurf.CreatedUtc));
        await initializer.MigrateAsync();
        await initializer.SeedAsync();
        Assert.Equal(first.Select(smurf => smurf.Id), (await ReadAll()).Select(smurf => smurf.Id));
    }

    /// <summary>
    /// Restarting preserves identifiers, all scalar values, nulls, and shared pet identity.
    /// </summary>
    [Fact]
    public async Task Commit_RoundTripsFieldsAndSharedPet()
    {
        var pet = new Pet { Name = "Azrael" };
        var known = new SmurfEntity
        {
            Name = "Known",
            Health = 42,
            Birthday = new DateTime(2000, 1, 2),
            Infected = false,
            Gender = Gender.Female,
            Pet = pet
        };
        var unknown = new SmurfEntity { Name = "Unknown", Pet = pet };
        await using (var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork())
        {
            await session.Smurfs.AddAsync(known);
            await session.Smurfs.AddAsync(unknown);
            await session.CompleteAsync();
        }

        var rows = await ReadAll();
        var saved = rows.Single(smurf => smurf.Id == known.Id);
        Assert.Equal(known.CreatedUtc, saved.CreatedUtc);
        Assert.Equal(DateTimeKind.Utc, saved.CreatedUtc.Kind);
        Assert.Equal(42, saved.Health);
        Assert.Equal(known.Birthday, saved.Birthday);
        Assert.Equal(DateTimeKind.Unspecified, saved.Birthday!.Value.Kind);
        Assert.Equal(false, saved.Infected);
        Assert.Equal(Gender.Female, saved.Gender);
        Assert.Equal(pet.Id, saved.PetId);
        Assert.Equal("Azrael", saved.Pet!.Name);
        var other = rows.Single(smurf => smurf.Id == unknown.Id);
        Assert.Same(saved.Pet, other.Pet);
        Assert.Null(other.Birthday);
        Assert.Null(other.Infected);
        Assert.Null(other.Gender);
    }

    /// <summary>
    /// Staged changes are visible within the session but only a completed session writes storage.
    /// </summary>
    [Fact]
    public async Task Dispose_DiscardsChangesAndDoesNotCreateFile()
    {
        await using (var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork())
        {
            await session.Smurfs.AddAsync(new SmurfEntity { Name = "Discarded" });
            Assert.Equal(1, await session.Smurfs.CountAsync([]));
            Assert.Empty(await ReadAll());
        }

        Assert.False(System.IO.File.Exists(CatalogPath));
        await Save(new SmurfEntity { Name = "Original" });
        await using (var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork())
        {
            var tracked = Assert.Single(await session.Smurfs.FindAsync([]));
            tracked.Name = "Discarded edit";
            Assert.Equal("Original", Assert.Single(await ReadAll()).Name);
        }

        Assert.Equal("Original", Assert.Single(await ReadAll()).Name);
    }

    /// <summary>
    /// Existing handlers can create, edit tracked entities, and delete through the file contracts.
    /// </summary>
    [Fact]
    public async Task ApplicationHandlers_CreateEditAndDeleteWithoutEfCore()
    {
        var services = new ServiceCollection().AddFilePersistence(CatalogPath);
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IUnitOfWorkFactory>();
        var catalog = new BusinessRuleCatalog();
        Assert.False((await new Create.Handler(factory, catalog).Handle(
            new Create.Command { Name = " " }, CancellationToken.None)).IsSuccess);
        Assert.True((await new Create.Handler(factory, catalog).Handle(
            new Create.Command { Name = "Original" }, CancellationToken.None)).IsSuccess);
        var original = Assert.Single(await ReadAll());
        Assert.True((await new Edit.Handler(factory, catalog).Handle(new Edit.Command
        {
            Id = original.Id,
            Name = "Edited",
            Health = 99,
            Infected = true,
            Gender = Gender.Male
        }, CancellationToken.None)).IsSuccess);
        var saved = Assert.Single(await ReadAll());
        Assert.Equal("Edited", saved.Name);
        Assert.Equal(99, saved.Health);
        Assert.True(saved.Infected);
        Assert.Equal(Gender.Male, saved.Gender);
        Assert.Equal(original.CreatedUtc, saved.CreatedUtc);
        await using var scope = provider.CreateAsyncScope();
        Assert.Equal(1, (await scope.ServiceProvider.GetRequiredService<ISmurfReadService>()
            .GetPageAsync(1, 25, CancellationToken.None)).TotalCount);
        Assert.True((await new Delete.Handler(factory).Handle(
            new Delete.Command { Id = saved.Id }, CancellationToken.None)).IsSuccess);
        Assert.Empty(await ReadAll());
    }

    /// <summary>
    /// Detached updates replace the selected row, and predicates are combined with AND.
    /// </summary>
    [Fact]
    public async Task Repository_UpdatesDetachedEntitiesAndCombinesPredicates()
    {
        var first = new SmurfEntity { Name = "First", Health = 10 };
        await Save(first, new SmurfEntity { Name = "Second", Health = 20 });
        await using var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork();
        Assert.Single(await session.Smurfs.FindAsync([smurf => smurf.Health >= 10, smurf => smurf.Name == "First"]));
        var replacement = new SmurfEntity(first.Id) { Name = "Replacement", CreatedUtc = first.CreatedUtc };
        await session.Smurfs.UpdateAsync(replacement);
        await session.CompleteAsync();
        Assert.Contains(await ReadAll(), smurf => smurf.Id == first.Id && smurf.Name == "Replacement");
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.Smurfs.AddAsync(replacement));
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.Smurfs.UpdateAsync(new SmurfEntity { Name = "Missing" }));
    }

    /// <summary>
    /// Filtering and sorting happen before page selection, preserving unknown infection values.
    /// </summary>
    [Fact]
    public async Task ReadService_FiltersSortsAndPagesSnapshot()
    {
        var baseline = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await Save(
            new SmurfEntity { Name = "Alpha", Infected = true, CreatedUtc = baseline },
            new SmurfEntity { Name = "Beta", Infected = false, CreatedUtc = baseline.AddDays(1) },
            new SmurfEntity { Name = "beta unknown", CreatedUtc = baseline.AddDays(2) },
            new SmurfEntity { Name = "Beta sick", Infected = true, CreatedUtc = baseline.AddDays(3) });
        var read = new FileReadService(new UnitOfWorkFactory(CatalogPath));
        var page = await read.GetPageAsync(1, 1, CancellationToken.None, " BeTA ", [false, null], newestFirst: true);
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
        Assert.Equal("beta unknown", Assert.Single(page).Name);
        Assert.Null(page[0].Infected);
        Assert.Equal("Beta", Assert.Single(await read.GetPageAsync(2, 1, CancellationToken.None, "beta", [false, null], true)).Name);
        Assert.Equal("Alpha", Assert.Single(await read.GetPageAsync(1, 1, CancellationToken.None)).Name);
        var beyond = await read.GetPageAsync(int.MaxValue, 50, CancellationToken.None);
        Assert.Empty(beyond);
        Assert.Equal(4, beyond.TotalCount);
        Assert.Equal(0, (await read.GetPageAsync(1, 50, CancellationToken.None, "%")).TotalCount);
    }

    /// <summary>
    /// A stale writer cannot overwrite a completed session from another factory.
    /// </summary>
    [Fact]
    public async Task ConflictingSessions_RejectStaleCommitWithoutLosingData()
    {
        await Save(new SmurfEntity { Name = "Original" });
        await using var first = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork();
        await using var stale = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork();
        (Assert.Single(await first.Smurfs.FindAsync([]))).Name = "Winner";
        (Assert.Single(await stale.Smurfs.FindAsync([]))).Name = "Stale";
        await first.CompleteAsync();
        await Assert.ThrowsAsync<FileConflictException>(() => stale.CompleteAsync());
        Assert.Equal("Winner", Assert.Single(await ReadAll()).Name);
        await first.Smurfs.AddAsync(new SmurfEntity { Name = "Second commit" });
        await first.CompleteAsync();
        Assert.Equal(2, (await ReadAll()).Count);
    }

    /// <summary>
    /// Cancellation and failed serialization preserve the old document and clean temporary files.
    /// </summary>
    [Fact]
    public async Task FailedCommit_PreservesOriginalBytes()
    {
        await Save(new SmurfEntity { Name = "Original" });
        var before = await System.IO.File.ReadAllTextAsync(CatalogPath);
        await using var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork();
        var tracked = Assert.Single(await session.Smurfs.FindAsync([]));
        tracked.Name = "Changed";
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.CompleteAsync(cancelled.Token));
        Assert.Equal(before, await System.IO.File.ReadAllTextAsync(CatalogPath));
        tracked.Gender = (Gender) 123;
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => session.CompleteAsync());
        Assert.Equal(before, await System.IO.File.ReadAllTextAsync(CatalogPath));
        Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
        tracked.Gender = null;
        await session.CompleteAsync();
        Assert.Equal("Changed", Assert.Single(await ReadAll()).Name);
    }

    /// <summary>
    /// Invalid or unsupported files are reported without being silently reset or reseeded.
    /// </summary>
    [Theory]
    [InlineData("{ broken")]
    [InlineData("{\"version\":99,\"revision\":0,\"smurfs\":[],\"pets\":[],\"villages\":[]}")]
    [InlineData("{\"version\":1,\"revision\":0}")]
    public async Task InvalidDocument_IsNeverReplaced(string contents)
    {
        Directory.CreateDirectory(_directory);
        await System.IO.File.WriteAllTextAsync(CatalogPath, contents);
        var initializer = new AppDataInitializer(CatalogPath);
        await Assert.ThrowsAsync<InvalidDataException>(() => initializer.MigrateAsync());
        await Assert.ThrowsAsync<InvalidDataException>(() => initializer.SeedAsync());
        Assert.Equal(contents, await System.IO.File.ReadAllTextAsync(CatalogPath));
    }

    /// <summary>
    /// Cancelled reads do not poison later reads, lock waits can be cancelled, and disposed sessions reject use.
    /// </summary>
    [Fact]
    public async Task CancellationAndDisposal_RespectSessionLifetime()
    {
        await Save(new SmurfEntity { Name = "Original" });
        var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork();
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.Smurfs.FindAsync([], cancelled.Token));
        Assert.Single(await session.Smurfs.FindAsync([]));
        await using (var locked = new FileStream(CatalogPath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.CompleteAsync(timeout.Token));
        }

        await session.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => session.Smurfs.FindAsync([]));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => session.CompleteAsync());
        Assert.Equal("Original", Assert.Single(await ReadAll()).Name);
    }

    /// <summary>
    /// Existing village records and memberships survive unrelated writes and shared pet edits.
    /// </summary>
    [Fact]
    public async Task Commit_PreservesVillageMembershipAndSharedPetEdits()
    {
        var pet = new Pet { Name = "Original pet" };
        await Save(new SmurfEntity { Name = "First", Pet = pet }, new SmurfEntity { Name = "Second", Pet = pet });
        var document = JsonNode.Parse(await System.IO.File.ReadAllTextAsync(CatalogPath))!.AsObject();
        var villageId = Guid.NewGuid();
        document["villages"]!.AsArray().Add(new JsonObject { ["id"] = villageId, ["name"] = "Village" });
        foreach (var row in document["smurfs"]!.AsArray())
        {
            row!["villageId"] = villageId;
        }

        await System.IO.File.WriteAllTextAsync(CatalogPath, document.ToJsonString());
        await using (var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork())
        {
            var rows = await session.Smurfs.FindAsync([]);
            rows[0].Pet!.Name = "Edited pet";
            rows[0].Name = "Edited";
            await session.CompleteAsync();
        }

        var saved = JsonNode.Parse(await System.IO.File.ReadAllTextAsync(CatalogPath))!;
        Assert.Equal(villageId, saved["villages"]![0]!["id"]!.GetValue<Guid>());
        Assert.All(saved["smurfs"]!.AsArray(), row => Assert.Equal(villageId, row!["villageId"]!.GetValue<Guid>()));
        Assert.All(await ReadAll(), row => Assert.Equal("Edited pet", row.Pet!.Name));
    }

    /// <summary>
    /// Removes only this test's isolated catalog directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private async Task Save(params SmurfEntity[] smurfs)
    {
        await using var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork();
        foreach (var smurf in smurfs)
        {
            await session.Smurfs.AddAsync(smurf);
        }

        await session.CompleteAsync();
    }

    private async Task<IReadOnlyList<SmurfEntity>> ReadAll()
    {
        await using var session = new UnitOfWorkFactory(CatalogPath).CreateUnitOfWork();
        return await session.Smurfs.FindAsync([]);
    }
}
