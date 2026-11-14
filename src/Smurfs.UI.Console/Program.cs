using CommandLine;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Craft.Domain;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Infrastructure.Logging;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.UI.Console.Verbs;
using Smurfs.Domain.BusinessRules;

namespace Smurfs.UI.Console;

internal class Program
{
    private const string DatabaseFile = "smurfs.db";

    private static Task<int> Main(string[] args)
    {
        return Parser.Default.ParseArguments<
                Verbs.Smurfs.Create,
                Verbs.Smurfs.List,
                InitializeDatabase>(args)
            .MapResult(
                (Verbs.Smurfs.Create options) => RunAsync(false, services => CreateSmurf(services, options)),
                (Verbs.Smurfs.List options) => RunAsync(true, services => ListSmurfs(services, options)),
                (InitializeDatabase _) => RunAsync(false, _ => Task.FromResult(0), initialize: true),
                errors => Task.FromResult(errors.All(error => error.StopsProcessing) ? 0 : 2));
    }

    private static async Task<int> RunAsync(
        bool readOnly,
        Func<IServiceProvider, Task<int>> run,
        bool initialize = false)
    {
        FrontendFileLogging? fileLogging = null;
        var startupComplete = false;
        try
        {
            fileLogging = new FrontendFileLogging("console");
            var initializeDatabase = initialize || !File.Exists(DatabaseFile);
            using var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    fileLogging.Configure(context.Configuration, logging);
                    logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
                    logging.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);
                })
                .ConfigureServices(services =>
                {
                    var connectionString = $"Data Source={DatabaseFile}";
                    if (readOnly && !initializeDatabase)
                    {
                        connectionString += ";Mode=ReadOnly";
                    }
                    services.AddAppDataPersistence<DataContext>(options => options.UseSqlite(connectionString));
                    services.AddApplication();
                    services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
                    services.AddScoped<ISmurfReadService, SmurfReadService>();
                    services.AddScoped<IBusinessRuleCatalog, BusinessRuleCatalog>();
                })
                .Build();

            await using var scope = host.Services.CreateAsyncScope();
            if (initializeDatabase)
            {
                await InitializeAsync(scope.ServiceProvider);
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

    private static async Task InitializeAsync(
        IServiceProvider services)
    {
        var initializer = services.GetRequiredService<IAppDataInitializer>();
        await initializer.MigrateAsync();
        await initializer.SeedAsync();
        System.Console.WriteLine("Database initialized.");
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
