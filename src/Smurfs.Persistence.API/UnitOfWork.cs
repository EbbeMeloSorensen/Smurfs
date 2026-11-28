using Smurfs.Persistence.API.Repositories;
using Smurfs.Persistence.VillageLife.Repositories;

namespace Smurfs.Persistence.API;

/// <summary>
/// Reserved HTTP write-adapter skeleton; not implemented or registered by the login/list milestone.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    /// <inheritdoc />
    public ISmurfRepository Smurfs { get; }

    /// <summary>
    /// Creates the reserved, unimplemented write-session skeleton.
    /// </summary>
    public UnitOfWork()
    {
        Smurfs = new SmurfRepository();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => throw new NotImplementedException();

    /// <inheritdoc />
    public Task CompleteAsync(
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
