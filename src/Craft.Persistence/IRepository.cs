namespace Craft.Persistence;

/// <summary>
/// Combines entity queries with staged mutations.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type managed by the repository.
/// </typeparam>
public interface IRepository<TEntity> :
    IReadRepository<TEntity>,
    IWriteRepository<TEntity>
    where TEntity : class
{
}
