using CommandLine;
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
        //args = "breakfast".Split();
        //args = "create --host localhost --user postgres --password L1on8Zebra --firstname Egon".Split();
        //args = "count --user john --password secret".Split();
        //args = "list --host localhost --user postgres --password L1on8Zebra".Split();
        //args = "import --host localhost --user postgres --password L1on8Zebra".Split();
        //args = "export --host localhost --user postgres --password L1on8Zebra".Split();
        //args = "update --user john --password secret --id 67".Split();
        //args = "delete --user john --password secret --id 67".Split();
        //args = "list -h localhost -d PR -u postgres -p L1on8Zebra".Split();
        //args = "import --filename Contacts.json --legacy true --host localhost --user postgres --password L1on8Zebra".Split();

        await Parser.Default.ParseArguments<
                //Verbs.PR.Create,
                //Count,
                //Verbs.PR.List,
                //Verbs.Smurfs.List,
                //Details,
                //Export,
                //Import,
                //Update,
                //Delete,
                Breakfast>(args)
            .MapResult(
                //(Verbs.PR.Create options) => CreatePerson(options),
                //(Count options) => CountPeople(options),
                //(Verbs.PR.List options) => ListPeople(options),
                //(Verbs.Smurfs.List options) => ListSmurfs(options),
                //(Details options) => GetPersonDetails(options),
                //(Export options) => ExportPeople(options),
                //(Import options) => ImportPeople(options),
                //(Update options) => UpdatePerson(options),
                //(Delete options) => DeletePerson(options),
                (Breakfast options) => MakeBreakfast(options),
                errs => Task.FromResult(0));
    }

    public static async Task MakeBreakfast(
        Breakfast options)
    {
        //System.Console.Write("Making breakfast...\nProgress: ");
        //await GetApplication().MakeBreakfast((progress, nameOfSubtask) =>
        //{
        //    System.Console.SetCursorPosition(10, System.Console.CursorTop);
        //    System.Console.Write($"{progress:F2} %");
        //    return false;
        //});
        //System.Console.WriteLine("\nDone");
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
                    var connectionString = "Data source=babuska3.db";

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
}
