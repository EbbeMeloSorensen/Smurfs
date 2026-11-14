using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Smurfs.API;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.Identity;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var identityContext = services.GetRequiredService<DataContext>();
            await identityContext.Database.MigrateAsync();

            var initializer = services.GetRequiredService<IAppDataInitializer>();
            await initializer.MigrateAsync();
            await initializer.SeedAsync();

            if (services.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                await Seed.SeedData(services.GetRequiredService<UserManager<AppUser>>());
            }
        }

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
