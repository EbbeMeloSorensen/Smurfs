using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence;

public interface IUnitOfWork : IDisposable
{
    ISmurfRepository Smurfs { get; }

    void Complete();
}