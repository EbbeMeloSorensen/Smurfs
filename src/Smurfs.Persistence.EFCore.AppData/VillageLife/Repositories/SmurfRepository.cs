using Craft.Persistence.EFCore;
using Microsoft.EntityFrameworkCore;
using Smurfs.Domain.VillageLife;
using Smurfs.Persistence.VillageLife.Repositories;

namespace Smurfs.Persistence.EFCore.AppData.VillageLife.Repositories;

/// <summary>
/// Applies the generic EF Core repository behavior to Smurfs.
/// </summary>
public class SmurfRepository : Repository<Smurf>, ISmurfRepository
{
    /// <summary>
    /// Creates a repository sharing an externally owned context.
    /// </summary>
    /// <param name="context">
    /// A context mapping Smurf entities.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// The context is null.
    /// </exception>
    public SmurfRepository(DbContext context) : base(context)
    {
    }
}

