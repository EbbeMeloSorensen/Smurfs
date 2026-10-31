using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    ISmurfRepository Smurfs { get; }

    Task CompleteAsync(
        CancellationToken cancellationToken = default);
}