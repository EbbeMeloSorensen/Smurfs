using CommandLine;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Craft.Domain;
using Smurfs.Persistence;
using Smurfs.Persistence.File;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Infrastructure.Logging;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.UI.Console.Verbs;
using Smurfs.Domain.VillageLife.BusinessRules;

namespace Smurfs.UI.Console;

internal class Program
{
    private static Task<int> Main(string[] args)
    {
        return Parser.Default.ParseArguments<
                Verbs.Smurfs.Create,
                Verbs.Smurfs.List,
                InitializeDatabase>(args)
            .MapResult(
                (Verbs.Smurfs.Create options) => RunAsync(services => CreateSmurf(services, options)),
                (Verbs.Smurfs.List options) => RunAsync(services => ListSmurfs(services, options)),
                (InitializeDatabase _) => RunAsync(_ => Task.FromResult(0), initialize: true),
                errors => Task.FromResult(errors.All(error => error.StopsProcessing) ? 0 : 2));
    }

    private static async Task<int> RunAsync(
        Func<IServiceProvider, Task<int>> run,
        bool initialize = false)
    {
        FrontendFileLogging? fileLogging = null;
        var startupComplete = false;
        try
        {
            fileLogging = new FrontendFileLogging("console");
            using var host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureLogging((context, logging) =>
                {
                    fileLogging.Configure(context.Configuration, logging);
                    logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
                    logging.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);
                })
                .ConfigureServices((context, services) =>
                {
                    var provider = context.Configuration["Database:Provider"];
                    switch (provider?.Trim().ToUpperInvariant())
                    {
                        case "SQLITE":
                            var sqliteConnection = GetRequiredConnectionString(context.Configuration, "Sqlite");
                            services.AddAppDataPersistence<SqliteDataContext>(options => options.UseSqlite(sqliteConnection));
                            break;
                        case "FILE":
                            var filePath = context.Configuration["FileStore:Path"] ?? "smurfs.json";
                            services.AddFilePersistence(Path.GetFullPath(filePath, context.HostingEnvironment.ContentRootPath));
                            break;
                        case "POSTGRES":
                            var postgresConnection = GetRequiredConnectionString(context.Configuration, "Postgres");
                            services.AddAppDataPersistence<PostgresDataContext>(options => options.UseNpgsql(postgresConnection));
                            break;
                        default:
                            throw new InvalidOperationException("Configure Database:Provider as Sqlite, Postgres or File.");
                    }

                    services.AddApplication();
                    if (!string.Equals(provider?.Trim(), "File", StringComparison.OrdinalIgnoreCase))
                    {
                        services.AddScoped<IUnitOfWorkFactory, Persistence.EFCore.AppData.UnitOfWorkFactory>();
                        services.AddScoped<ISmurfReadService, SmurfReadService>();
                    }
                    services.AddScoped<IBusinessRuleCatalog, BusinessRuleCatalog>();
                })
                .Build();

            await using var scope = host.Services.CreateAsyncScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var isFile = string.Equals(configuration["Database:Provider"]?.Trim(), "File", StringComparison.OrdinalIgnoreCase);
            var storageMissing = isFile
                ? !System.IO.File.Exists(Path.GetFullPath(configuration["FileStore:Path"] ?? "smurfs.json", AppContext.BaseDirectory))
                : !await scope.ServiceProvider.GetRequiredService<DataContext>()
                    .GetService<IRelationalDatabaseCreator>().ExistsAsync();
            if (initialize || storageMissing)
            {
                await InitializeAsync(scope.ServiceProvider, initialize);
            }

            startupComplete = true;
            return await run(scope.ServiceProvider);
        }
        catch (Exception exception)
        {
            if (!startupComplete)
            {
                fileLogging?.LogStartupFailure<Program>(exception);
            }

            System.Console.Error.WriteLine(exception.Message);
            return 1;
        }
        finally
        {
            fileLogging?.Dispose();
        }
    }

    private static string GetRequiredConnectionString(IConfiguration config, string name)
    {
        var connectionString = config.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Configure ConnectionStrings:{name}.");
        }

        return connectionString;
    }

    private static async Task InitializeAsync(
        IServiceProvider services, bool reportInitialization)
    {
        var initializer = services.GetRequiredService<IAppDataInitializer>();
        await initializer.MigrateAsync();
        await initializer.SeedAsync();
        if (reportInitialization)
        {
            System.Console.WriteLine("Application data initialized.");
        }
    }

    private static async Task<int> CreateSmurf(
        IServiceProvider services,
        Verbs.Smurfs.Create options)
    {
        var sender = services.GetRequiredService<ISender>();
        var result = await sender.Send(new Create.Command
        {
            Name = options.Name
        });

        if (!result.IsSuccess)
        {
            System.Console.Error.WriteLine(result.Error);
            return 1;
        }

        System.Console.WriteLine($"Smurf: \"{options.Name}\" created successfully.");
        return 0;
    }

    private static async Task<int> ListSmurfs(
        IServiceProvider services,
        Verbs.Smurfs.List options)
    {
        var sender = services.GetRequiredService<ISender>();
        var result = await sender.Send(new List.Query
        {
            Params = new SmurfParams { PageNumber = options.Page, PageSize = options.PageSize }
        });

        if (!result.IsSuccess)
        {
            System.Console.Error.WriteLine(result.Error);
            return 1;
        }

        var page = result.Value;
        System.Console.WriteLine($"Smurfs (page {page.CurrentPage}, {page.TotalPages} total pages, {page.TotalCount} rows):");
        foreach (var smurf in page)
        {
            System.Console.WriteLine($" {smurf.Name}");
        }

        return 0;
    }
}
