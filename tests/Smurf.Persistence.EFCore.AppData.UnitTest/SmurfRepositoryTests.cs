using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.Persistence.EFCore.AppData.Repositories;

namespace Smurf.Persistence.EFCore.AppData.UnitTest
{
    public class SmurfRepositoryTests
    {
        [Fact]
        public async Task Add_ShouldPersistSmurf()
        {
            // Arrange
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            await using (var context = new DataContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var repository = new SmurfRepository(context);

                var smurf = new Smurfs.Domain.Smurf
                {
                    Name = "Papa Smurf"
                };

                // Act
                await repository.AddAsync(smurf);
                await context.SaveChangesAsync();
            }

            // Assert - use a fresh DbContext
            await using (var context = new DataContext(options))
            {
                var savedSmurf = await context.Set<Smurfs.Domain.Smurf>()
                    .SingleAsync();

                Assert.Equal("Papa Smurf", savedSmurf.Name);
            }
        }
    }
}
