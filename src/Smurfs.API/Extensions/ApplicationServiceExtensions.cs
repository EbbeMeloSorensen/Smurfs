using Craft.Domain;
using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Npgsql;
using Smurfs.Application.Core;
using Smurfs.Application.Interfaces;
using Smurfs.Application.Smurfs;
using Smurfs.Domain.BusinessRules;
using Smurfs.Infrastructure.Security;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurfs.API.Extensions;

/// <summary>
/// Provides service registration for the Smurfs API.
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registers persistence, application handlers, and HTTP support services using the API configuration.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
        });

        var connectionString = string.Empty;

        // When launching the API from Visual Studio or VS Code, this environment variable is taken
        // from the files launchSettings.json, where it is set to Development.
        // When deploying to Heroku, it is taken from the application parameter section in Heroku,
        // where it should be set to Production

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Console.WriteLine($"\nENVIRONMENT IS: {env}!!!\n");

        // Depending on if in development or production, use either Heroku-provided
        // connection string, or development connection string from env var.
        if (env == "Development")
        {
            Console.WriteLine("\nReading connection string from appsettings.json\n");

            // Use connection string from file.
            connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.");
        }
        else
        {
            Console.WriteLine("\nReading connection string from Heroku config variable: DATABASE_URL\n");

            connectionString = GetHerokuConnectionString(Environment.GetEnvironmentVariable("DATABASE_URL"));
        }

        services.AddDbContext<Smurfs.Persistence.EFCore.Identity.DataContext>(options =>
                //options.UseSqlite(connectionString));
                options.UseNpgsql(connectionString));

        services.AddAppDataPersistence<Smurfs.Persistence.EFCore.AppData.DataContext>(options =>
            //options.UseSqlite(connectionString));
            options.UseNpgsql(connectionString));

        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:3000");
            });
        });

        services.AddApplication();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<IUnitOfWorkFactory, Persistence.EFCore.AppData.UnitOfWorkFactory>();
        services.AddScoped<ISmurfReadService, SmurfReadService>();
        services.AddScoped<IBusinessRuleCatalog, BusinessRuleCatalog>();

        return services;
    }

    private static string GetHerokuConnectionString(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            throw new InvalidOperationException("Configure the DATABASE_URL environment variable.");
        }

        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("postgres" or "postgresql")
            || string.IsNullOrEmpty(uri.Host)
            || uri.Port == 0
            || uri.AbsolutePath.Length <= 1
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new InvalidOperationException("DATABASE_URL must be a PostgreSQL URL with a host and database name.");
        }

        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
        {
            throw new InvalidOperationException("DATABASE_URL must include a PostgreSQL username and password.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.IdnHost,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath[1..]),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1]),
            SslMode = SslMode.Require
        }.ConnectionString;
    }
}
