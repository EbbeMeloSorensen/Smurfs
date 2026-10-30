using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smurfs.Domain;

namespace Smurfs.Persistence.EFCore.AppData.EntityConfigurations;

public class SmurfConfiguration : IEntityTypeConfiguration<Smurf>
{
    public void Configure(EntityTypeBuilder<Smurf> builder)
    {
        builder.HasKey(s => s.Id);
    }
}

