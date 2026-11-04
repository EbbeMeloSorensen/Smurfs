using Microsoft.EntityFrameworkCore;
using Smurfs.Domain;
using Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// EF Core persistence session exposing Smurfs, Villages and Pets with explicit Smurf relationship configuration.
/// </summary>
/// <remarks>
/// Village membership uses an optional shadow VillageId foreign key on Smurf. OnModelCreating invokes Configure to apply the explicit Smurf mapping.
/// </remarks>
public class DataContext : DbContext
{
    /// <summary>
    /// Gets or sets the Smurf entity set; queries track by default.
    /// </summary>
    public DbSet<Smurf> Smurfs { get; set; }

    /// <summary>
    /// Gets or sets the Village entity set; queries track by default.
    /// </summary>
    public DbSet<Village> Villages { get; set; }

    /// <summary>
    /// Gets or sets the Pet entity set; queries track by default.
    /// </summary>
    public DbSet<Pet> Pets { get; set; }

    /// <summary>
    /// Creates a context with caller-supplied provider and options.
    /// </summary>
    /// <param name="options">
    /// The EF Core configuration for this context.
    /// </param>
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Applies the explicit Smurf configuration to a model builder.
    /// </summary>
    /// <param name="modelBuilder">
    /// The model builder to configure.
    /// </param>
    /// <remarks>
    /// Called by OnModelCreating; also available to callers composing the same model.
    /// </remarks>
    public static void Configure(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SmurfConfiguration());
    }

    /// <summary>
    /// Applies the explicit entity configurations during EF Core model creation.
    /// </summary>
    /// <param name="modelBuilder">
    /// The model builder supplied by EF Core.
    /// </param>
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        Configure(modelBuilder);
    }
}
