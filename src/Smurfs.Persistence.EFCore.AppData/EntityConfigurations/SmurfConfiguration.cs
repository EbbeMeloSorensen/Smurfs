using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smurfs.Domain;

namespace Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

/// <summary>
/// Defines the Smurf primary key and optional many-to-one Pet relationship.
/// </summary>
/// <remarks>
/// Pet deletion uses Restrict: database references prevent deletion unless cleared first.
/// EF can clear nullable PetId values on tracked Smurfs before deleting their Pet; this is not a domain-level deletion prohibition.
/// </remarks>
public class SmurfConfiguration : IEntityTypeConfiguration<Smurf>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Smurf> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Pet)
            .WithMany()
            .HasForeignKey(s => s.PetId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

