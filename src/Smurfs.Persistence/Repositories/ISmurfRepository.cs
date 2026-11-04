using Craft.Persistence;
using Smurfs.Domain;

namespace Smurfs.Persistence.Repositories;

/// <summary>
/// Queries Smurfs and stages their mutations within an owning persistence session.
/// </summary>
public interface ISmurfRepository : IRepository<Smurf>
{
}
