using Smurfs.Persistence.API.Repositories;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.API;

public sealed class UnitOfWork : IUnitOfWork
{
    public UnitOfWork()
    {
        Smurfs = new SmurfRepository();
    }

    public ValueTask DisposeAsync() => throw new NotImplementedException();

    public ISmurfRepository Smurfs { get; }

    public Task CompleteAsync(
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
