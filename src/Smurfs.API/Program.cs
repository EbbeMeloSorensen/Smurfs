using Microsoft.EntityFrameworkCore;
using Smurfs.API;
using Smurfs.Persistence;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var identityContext = services.GetRequiredService<Smurfs.Persistence.EFCore.Identity.DataContext>();
            await identityContext.Database.MigrateAsync();

            var initializer = services.GetRequiredService<IAppDataInitializer>();
            await initializer.MigrateAsync();
            await initializer.SeedAsync();
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
