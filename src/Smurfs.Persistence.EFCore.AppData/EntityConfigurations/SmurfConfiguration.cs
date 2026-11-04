using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smurfs.Domain;

namespace Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

/// <summary>
/// Defines the explicit Smurf primary-key mapping.
/// </summary>
public class SmurfConfiguration : IEntityTypeConfiguration<Smurf>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Smurf> builder)
    {
        builder.HasKey(s => s.Id);
    }
}

