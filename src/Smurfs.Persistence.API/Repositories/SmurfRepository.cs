using System.Linq.Expressions;
using Smurfs.Domain;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.API.Repositories;

/// <summary>
/// Reserved HTTP write-adapter skeleton; not implemented or registered by the login/list milestone.
/// </summary>
public class SmurfRepository : ISmurfRepository
{
    /// <inheritdoc />
    public Task<int> CountAsync(
        IEnumerable<Expression<Func<Smurf, bool>>> predicates,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    /// <inheritdoc />
    public Task<IReadOnlyList<Smurf>> FindAsync(
        IEnumerable<Expression<Func<Smurf, bool>>> predicates,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    /// <inheritdoc />
    public Task AddAsync(
        Smurf entity,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    /// <inheritdoc />
    public Task UpdateAsync(
        Smurf entity,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    /// <inheritdoc />
    public Task RemoveAsync(
        Smurf entity,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
}
