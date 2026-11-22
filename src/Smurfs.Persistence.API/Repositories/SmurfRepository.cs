using System.Linq.Expressions;
using Smurfs.Domain;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.API.Repositories;

public class SmurfRepository : ISmurfRepository
{
    public Task<int> CountAsync(
        IEnumerable<Expression<Func<Smurf, bool>>> predicates,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<IReadOnlyList<Smurf>> FindAsync(
        IEnumerable<Expression<Func<Smurf, bool>>> predicates,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task AddAsync(
        Smurf entity,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task UpdateAsync(
        Smurf entity,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task RemoveAsync(
        Smurf entity,
        CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
}
