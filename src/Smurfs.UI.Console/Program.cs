using CommandLine;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smurfs.Application.Core;
using Smurfs.Application.Interfaces;
using Smurfs.Infrastructure.Pagination;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.UI.Console.Verbs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Smurfs.UI.Console;

internal class Program
{
    private static IHost? _host = null;

    static async Task Main(
        string[] args)
    {
        await Parser.Default.ParseArguments<
                Verbs.Smurfs.Create,
                Verbs.Smurfs.List>(args)
            .MapResult(
                (Verbs.Smurfs.Create options) => CreateSmurf(options),
                (Verbs.Smurfs.List options) => ListSmurfs(options),
                errs => Task.FromResult(0));
    }

    private static async Task<IHost> GetHost()
    {
        if (_host == null)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    // Turn off logging entirely
                    //logging.ClearProviders();

                    // Filter EF Core logs
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
                })
                .ConfigureServices((context, services) =>
                {
                    // Load connection string from appsettings.json or environment
                    //var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    var connectionString = "Data source=smurfs.db";

                    // Postgres - MELO - Basement
                    //connectionString = "Server=localhost;Port=5432;User Id=root;Password=root;Database=DB_Temple_UI_Console";

                    // Postgres - Linux in podman
                    //connectionString = "Server=localhost;Port=5432;User Id=myuser;Password=mypassword;Database=DB_Temple_UI_Console";

                    services.AddAppDataPersistence<DataContext>(options =>
                    {
                        options.UseSqlite(connectionString);
                        //options.UseNpgsql(connectionString);
                    });

                    services.AddApplication(); // registers MediatR and handlers
                    services.AddAutoMapper(assemblies: typeof(MappingProfiles).Assembly);
                    services.AddScoped<IUserAccessor, UserAccessor>();
                    services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
                    //services.AddScoped<IBusinessRuleCatalog, BusinessRuleCatalog>();
                    services.AddScoped<IPagingHandler<Application.Smurfs.SmurfDto>, PagingHandler<Application.Smurfs.SmurfDto>>();
                })
                .Build();

            // Run migrations once when the host is created
            using var scope = _host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            await db.Database.MigrateAsync();
            await Seeding.SeedDatabase(db);
        }

        return _host;
    }

    public static async Task CreateSmurf(
        Verbs.Smurfs.Create options)
    {
        System.Console.Write("Creating Smurf...\nProgress: ");

        var smurf = new Domain.Smurf
        {
            Name = options.Name,
            Age = 0,
        };

        try
        {
            using var scope = (await GetHost()).Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new Application.Smurfs.Create.Command
                {
                    Smurf = smurf
                });

            System.Console.Write(result.IsSuccess
                ? $"\nSmurf: \"{smurf.Name}\" created successfully\n"
                : $"\nSmurf: \"{smurf.Name}\" creation failed ({result.Error})\n");
        }
        catch (Exception exception)
        {
            System.Console.Write($"\nException thrown when trying to create smurf: \"{exception.Message}\"\n");
        }
    }

    public static async Task ListSmurfs(
        Verbs.Smurfs.List options)
    {
        try
        {
            using var scope = (await GetHost()).Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new Application.Smurfs.List.Query { Params = new Application.Smurfs.SmurfParams() });

            System.Console.WriteLine($"\nSmurfs:");
            foreach (var smurfDto in result.Value)
            {
                System.Console.WriteLine($" {smurfDto.Name}");
            }
        }
        catch (HttpRequestException exception)
        {
            System.Console.Write($"\nHttpRequestException thrown: \"{exception.Message}\"\n");
        }
        catch (Exception exception)
        {
            System.Console.Write($"\nException thrown: \"{exception.Message}\"\n");
        }
    }

}
