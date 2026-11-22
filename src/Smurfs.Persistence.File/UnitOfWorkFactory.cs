namespace Smurfs.Persistence.File;

/// <summary>
/// Creates independent sessions for one catalog path.
/// </summary>
public sealed class UnitOfWorkFactory(string filePath) : IUnitOfWorkFactory
{
    private readonly FileStore _store = new(filePath);

    /// <inheritdoc />
    public IUnitOfWork CreateUnitOfWork() => new UnitOfWork(_store);
}
