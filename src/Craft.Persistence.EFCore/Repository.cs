using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Craft.Persistence.EFCore
{
    /// <summary>Uses an EF Core context to query entities and stage changes.</summary>
    /// <typeparam name="TEntity">An entity type mapped by the supplied context.</typeparam>
    /// <remarks>Does not own or dispose the context. Queries use its configured tracking policy (tracking by default), apply predicates with AND, and do not explicitly include navigations. Mutations use EF graph/state rules; Update may mark all mapped values modified and attach reachable entities. No method saves changes.</remarks>
    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>The context shared with the owning persistence session.</summary>
        protected readonly DbContext Context;

        /// <summary>Creates a repository using an externally owned context.</summary>
        /// <param name="context">The context used for all operations.</param>
        /// <exception cref="ArgumentNullException">The context is null.</exception>
        protected Repository(
            DbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyPredicates(predicates);

            return await query.CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TEntity>> FindAsync(
            IEnumerable<Expression<Func<TEntity, bool>>> predicates,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyPredicates(predicates);

            return await query.ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Context.Set<TEntity>().Add(entity);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task UpdateAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Context.Set<TEntity>().Update(entity);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
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