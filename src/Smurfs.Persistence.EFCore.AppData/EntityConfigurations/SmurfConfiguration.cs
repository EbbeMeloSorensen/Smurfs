using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smurfs.Domain;

namespace Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

/// <summary>
/// Defines the Smurf primary key and optional many-to-one Pet and Village relationships.
/// </summary>
/// <remarks>
/// Pet deletion uses Restrict: database references prevent deletion unless cleared first.
/// EF can clear nullable PetId values on tracked Smurfs before deleting their Pet; this is not a domain-level deletion prohibition.
/// Village membership uses a nullable shadow VillageId with no Village navigation on Smurf.
/// ClientSetNull preserves the existing optional membership deletion behavior.
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

        builder.HasOne<Village>()
            .WithMany(v => v.Smurfs)
            .HasForeignKey("VillageId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}

