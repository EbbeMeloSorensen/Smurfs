using Smurfs.Persistence.EFCore.AppData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Craft.Domain;
using Smurfs.Application.Core;
using Smurfs.Application.Interfaces;
using Smurfs.Application.Smurfs;
using Smurfs.Domain.BusinessRules;
using Smurfs.Infrastructure.Security;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence;

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

        services.AddDbContext<Smurfs.Persistence.EFCore.Identity.DataContext>(options =>
            options.UseSqlite(config.GetConnectionString("Identity")
                ?? throw new InvalidOperationException("Configure ConnectionStrings:Identity.")));

        services.AddAppDataPersistence<Smurfs.Persistence.EFCore.AppData.DataContext>(options =>
            options.UseSqlite(config.GetConnectionString("AppData")
                ?? throw new InvalidOperationException("Configure ConnectionStrings:AppData.")));

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
