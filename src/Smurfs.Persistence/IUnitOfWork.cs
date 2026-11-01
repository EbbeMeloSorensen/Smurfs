using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence;

/// <summary>Owns repositories and a commit boundary for one persistence session.</summary>
/// <remarks>Dispose asynchronously after use. Transaction guarantees depend on the implementation; disposal does not implicitly commit.</remarks>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Gets the Smurf repository participating in this session.</summary>
    ISmurfRepository Smurfs { get; }

    /// <summary>Persists changes staged in this session.</summary>
    /// <param name="cancellationToken">Requests cancellation of the commit.</param>
    /// <returns>A task representing the commit operation.</returns>
    Task CompleteAsync(
        CancellationToken cancellationToken = default);
}
