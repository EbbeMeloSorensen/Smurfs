using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>Creates SQLite contexts for EF Core design-time tooling.</summary>
/// <remarks>Uses smurfs.db relative to the process working directory. This is separate from the runtime UnitOfWorkFactory.</remarks>
public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    /// <summary>Creates a caller-owned SQLite context without opening or migrating its database.</summary>
    /// <param name="args">Tooling arguments; currently ignored.</param>
    /// <returns>A context that the caller must dispose.</returns>
    public DataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite("Data Source=smurfs.db")
            .Options;

        return new DataContext(options);
    }
}
