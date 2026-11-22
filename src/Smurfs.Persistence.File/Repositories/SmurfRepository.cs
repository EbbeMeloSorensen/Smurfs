using System.Linq.Expressions;
using Smurfs.Domain;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.File.Repositories;

/// <summary>
/// Queries tracked session entities and stages changes without writing the file.
/// Queries include changes already staged in the same session.
/// </summary>
public sealed class SmurfRepository(UnitOfWork session) : ISmurfRepository
{
    /// <inheritdoc />
    public async Task<int> CountAsync(
        IEnumerable<Expression<Func<Smurf, bool>>> predicates,
        CancellationToken cancellationToken = default)
        => (await FindAsync(predicates, cancellationToken)).Count;

    /// <inheritdoc />
    public async Task<IReadOnlyList<Smurf>> FindAsync(
        IEnumerable<Expression<Func<Smurf, bool>>> predicates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicates);
        var catalog = await session.GetCatalogAsync(cancellationToken);
        IEnumerable<Smurf> matches = catalog.Smurfs.Values;
        foreach (var predicate in predicates)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            matches = matches.Where(predicate.Compile());
        }

        var result = matches.ToArray();
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    /// <inheritdoc />
    public async Task AddAsync(Smurf entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var catalog = await session.GetCatalogAsync(cancellationToken);
        if (catalog.Smurfs.ContainsKey(entity.Id))
        {
            throw new InvalidOperationException("A Smurf with this identifier already exists.");
        }

        catalog.AttachPet(entity);
        catalog.Smurfs.Add(entity.Id, entity);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Smurf entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var catalog = await session.GetCatalogAsync(cancellationToken);
        if (!catalog.Smurfs.ContainsKey(entity.Id))
        {
            throw new InvalidOperationException("Smurf not found.");
        }

        catalog.AttachPet(entity);
        catalog.Smurfs[entity.Id] = entity;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Smurf entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var catalog = await session.GetCatalogAsync(cancellationToken);
        catalog.Smurfs.Remove(entity.Id);
        catalog.Memberships.Remove(entity.Id);
    }
}
