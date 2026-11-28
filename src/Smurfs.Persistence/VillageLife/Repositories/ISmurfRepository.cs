using Craft.Persistence;
using Smurfs.Domain.VillageLife;

namespace Smurfs.Persistence.VillageLife.Repositories;

/// <summary>
/// Queries Smurfs and stages their mutations within an owning persistence session.
/// </summary>
public interface ISmurfRepository : IRepository<Smurf>
{
}
