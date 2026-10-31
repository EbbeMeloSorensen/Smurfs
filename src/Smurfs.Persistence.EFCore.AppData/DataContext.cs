using Microsoft.EntityFrameworkCore;
using Smurfs.Domain;
using Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>EF Core persistence session exposing Smurfs and discovering their Pet relationship by convention.</summary>
/// <remarks>Village is not mapped. The static Configure helper is not automatically invoked during model creation.</remarks>
public class DataContext : DbContext
{
    /// <summary>Gets or sets the Smurf entity set; queries track by default.</summary>
    public DbSet<Smurf> Smurfs { get; set; }

    /// <summary>Creates a context with caller-supplied provider and options.</summary>
    /// <param name="options">The EF Core configuration for this context.</param>
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    /// <summary>Applies the explicit Smurf configuration to a model builder.</summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <remarks>This helper has no call site in this solution; normal context model creation currently uses conventions.</remarks>
    public static void Configure(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SmurfConfiguration());
    }
}
