using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Creates a DataContext for Entity Framework Core design-time operations,
/// such as generating and applying migrations.
/// </summary>
public sealed class DataContextFactory
    : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite("Data Source=smurfs-identity.db")
            .Options;

        return new DataContext(options);
    }
}
