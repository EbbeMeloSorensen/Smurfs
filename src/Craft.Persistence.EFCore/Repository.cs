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

        public Task Add(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}