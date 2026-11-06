using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smurfs.Persistence.EFCore.AppData;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppDataPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
        where TContext : DataContext
    {
        services.AddDbContextFactory<TContext>(optionsAction);
        services.AddScoped<IAppDataInitializer, AppDataInitializer<TContext>>();

        // register repositories, unit of work, etc.
        //services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

