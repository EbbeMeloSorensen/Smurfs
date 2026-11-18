using Craft.Domain;
using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Smurfs.Application.Core;
using Smurfs.Application.Interfaces;
using Smurfs.Application.Smurfs;
using Smurfs.Domain.BusinessRules;
using Smurfs.Infrastructure.Security;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurfs.API.Extensions;

public static class ApplicationServiceExtensions
{
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

            // Use connection string provided at runtime by Heroku.
            var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            // Parse connection URL to connection string for Npgsql
            connUrl = connUrl.Replace("postgres://", string.Empty);
            var pgUserPass = connUrl.Split("@")[0];
            var pgHostPortDb = connUrl.Split("@")[1];
            var pgHostPort = pgHostPortDb.Split("/")[0];
            var pgDb = pgHostPortDb.Split("/")[1];
            var pgUser = pgUserPass.Split(":")[0];
            var pgPass = pgUserPass.Split(":")[1];
            var pgHost = pgHostPort.Split(":")[0];
            var pgPort = pgHostPort.Split(":")[1];

            connectionString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb}; SSL Mode=Require; Trust Server Certificate=true";
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
        services.AddAutoMapper(assemblies: typeof(MappingProfiles).Assembly);
        services.AddHttpContextAccessor();
        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<IUnitOfWorkFactory, Persistence.EFCore.AppData.UnitOfWorkFactory>();
        services.AddScoped<ISmurfReadService, SmurfReadService>();
        services.AddScoped<IBusinessRuleCatalog, BusinessRuleCatalog>();

        return services;
    }
}
