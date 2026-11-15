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

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.");

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
