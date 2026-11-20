using Craft.Domain;
using Microsoft.EntityFrameworkCore;
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
using Smurfs.Persistence.EFCore.Identity;

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

        var provider = config["Database:Provider"];

        switch (provider?.Trim().ToUpperInvariant())
        {
            case "SQLITE":
                var sqliteConnection = GetRequiredConnectionString(config, "Sqlite");
                services.AddIdentityPersistence<Smurfs.Persistence.EFCore.Identity.SqliteDataContext>(options =>
                    options.UseSqlite(sqliteConnection));
                services.AddAppDataPersistence<Smurfs.Persistence.EFCore.AppData.SqliteDataContext>(options =>
                    options.UseSqlite(sqliteConnection));
                break;
            case "POSTGRES":
                var databaseUrl = config["DATABASE_URL"];
                var postgresConnection = string.IsNullOrWhiteSpace(databaseUrl)
                    ? GetRequiredConnectionString(config, "Postgres")
                    : GetHerokuConnectionString(databaseUrl);
                services.AddIdentityPersistence<Smurfs.Persistence.EFCore.Identity.PostgresDataContext>(options =>
                    options.UseNpgsql(postgresConnection));
                services.AddAppDataPersistence<Smurfs.Persistence.EFCore.AppData.PostgresDataContext>(options =>
                    options.UseNpgsql(postgresConnection));
                break;
            default:
                throw new InvalidOperationException("Configure Database:Provider as Sqlite or Postgres.");
        }

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

    private static string GetRequiredConnectionString(IConfiguration config, string name)
    {
        var connectionString = config.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Configure ConnectionStrings:{name}.");
        }

        return connectionString;
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
