using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Craft.Persistence.EFCore
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext Context;

        protected Repository(
            DbContext context)
        {
            Context = context;
        }

        public Task<IEnumerable<TEntity>> Find(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates)
        {
            throw new NotImplementedException();
        }

        public virtual async Task Add(TEntity entity)
        {
            await Task.Run(() =>
            {
                Context.Set<TEntity>().Add(entity);
            });
        }
    }
}