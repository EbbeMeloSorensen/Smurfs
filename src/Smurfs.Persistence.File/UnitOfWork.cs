using Smurfs.Persistence.Repositories;
using Smurfs.Persistence.File.Repositories;

namespace Smurfs.Persistence.File;

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
