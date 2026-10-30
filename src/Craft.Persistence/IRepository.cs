using System.Linq.Expressions;

namespace Craft.Persistence;

public interface IRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> Find(
        IEnumerable<Expression<Func<TEntity, bool>>> predicates);

    Task Add(TEntity entity);
}
