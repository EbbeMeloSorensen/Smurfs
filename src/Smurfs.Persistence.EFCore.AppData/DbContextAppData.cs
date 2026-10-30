using Microsoft.EntityFrameworkCore;
using Smurfs.Domain;
using Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

namespace Smurfs.Persistence.EFCore.AppData;

public class DbContextAppData : DbContext
{
    public DbSet<Smurf> Smurfs { get; set; }

    public static void Configure(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SmurfConfiguration());
    }
}
