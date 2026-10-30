using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Smurfs.Persistence.EFCore.AppData;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite("Data Source=smurfs.db")
            .Options;

        return new DataContext(options);
    }
}