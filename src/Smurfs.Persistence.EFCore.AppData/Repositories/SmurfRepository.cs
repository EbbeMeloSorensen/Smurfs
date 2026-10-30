using Microsoft.EntityFrameworkCore;
using Craft.Persistence.EFCore;
using Smurfs.Domain;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.EFCore.AppData.Repositories;

public class SmurfRepository : Repository<Smurf>, ISmurfRepository
{
    public SmurfRepository(DbContext context) : base(context)
    {
    }
}

