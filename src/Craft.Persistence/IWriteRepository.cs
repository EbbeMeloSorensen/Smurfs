namespace Craft.Persistence;

public interface IWriteRepository<TEntity> where TEntity : class
{
    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);
}