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

        public Task<int> CountAsync(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<TEntity>> FindAsync(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                Context.Set<TEntity>().Add(entity);
            }, cancellationToken);
        }

        public Task UpdateAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}