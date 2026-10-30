using Microsoft.EntityFrameworkCore;
using Smurfs.Domain;
using Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

namespace Smurfs.Persistence.EFCore.AppData;

public class DataContext : DbContext
{
    public DbSet<Smurf> Smurfs { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public static void Configure(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SmurfConfiguration());
    }
}
