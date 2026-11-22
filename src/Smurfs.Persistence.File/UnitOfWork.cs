using Smurfs.Persistence.File.Repositories;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.File;

/// <summary>
/// Owns an independent file snapshot and commits changes only on request.
/// Instances and their tracked entities must not be used concurrently.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FileStore _store;

    private FileCatalog? _catalog;

    private bool _disposed;

    /// <inheritdoc />
    public ISmurfRepository Smurfs { get; }

    /// <summary>
    /// Creates a session for the configured catalog file.
    /// </summary>
    public UnitOfWork(string filePath) : this(new FileStore(filePath))
    {
    }

    internal UnitOfWork(FileStore store)
    {
        _store = store;
        Smurfs = new SmurfRepository(this);
    }

    /// <summary>
    /// Saves the complete snapshot with atomic replacement after checking its revision.
    /// </summary>
    /// <exception cref="FileConflictException">
    /// Another session committed since this snapshot was loaded. Open a new session and retry.
    /// </exception>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await GetCatalogAsync(cancellationToken);
        await _store.SaveAsync(catalog, cancellationToken);
    }

    /// <summary>
    /// Discards uncommitted state and invalidates the session.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _disposed = true;
        _catalog = null;
        return ValueTask.CompletedTask;
    }

    internal async Task<FileCatalog> GetCatalogAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        _catalog ??= await _store.ReadAsync(cancellationToken);
        return _catalog;
    }
}
