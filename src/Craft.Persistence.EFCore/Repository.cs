using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Craft.Persistence.EFCore
{
    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected readonly DbContext Context;

        protected Repository(
            DbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> CountAsync(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyPredicates(predicates);

            return await query.CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> FindAsync(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyPredicates(predicates);

            return await query.ToListAsync(cancellationToken);
        }

        public Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Context.Set<TEntity>().Add(entity);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Context.Set<TEntity>().Update(entity);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Context.Set<TEntity>().Remove(entity);

            return Task.CompletedTask;
        }

        private IQueryable<TEntity> ApplyPredicates(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            return query;
        }
    }
}