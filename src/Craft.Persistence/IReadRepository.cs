using System.Linq.Expressions;

namespace Craft.Persistence;

public interface IReadRepository<TEntity> where TEntity : class
{
    Task<int> CountAsync(
        IEnumerable<Expression<Func<TEntity, bool>>> predicates,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FindAsync(
        IEnumerable<Expression<Func<TEntity, bool>>> predicates,
        CancellationToken cancellationToken = default);
}