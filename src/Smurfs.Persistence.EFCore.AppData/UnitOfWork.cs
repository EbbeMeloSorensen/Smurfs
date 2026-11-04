using Smurfs.Persistence.EFCore.AppData.Repositories;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Coordinates Smurf operations using one owned EF Core context.
/// </summary>
/// <remarks>
/// CompleteAsync delegates to SaveChangesAsync; disposal delegates to context disposal without saving. Do not use this session concurrently.
/// </remarks>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    /// <inheritdoc />
    public ISmurfRepository Smurfs { get; }

    /// <summary>
    /// Takes ownership of the context and creates a repository sharing it.
    /// </summary>
    /// <param name="context">
    /// The context to commit and dispose.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// The context is null.
    /// </exception>
    public UnitOfWork(
        DataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        Smurfs = new SmurfRepository(_context);
    }

    /// <inheritdoc />
    /// <remarks>
    /// EF Core persistence, concurrency and cancellation exceptions propagate to the caller.
    /// </remarks>
    public async Task CompleteAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously disposes the owned context without committing staged changes.
    /// </summary>
    /// <returns>
    /// The context disposal operation.
    /// </returns>
    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}
