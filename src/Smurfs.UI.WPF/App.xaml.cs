using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Infrastructure.Pagination;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.ViewModel;

namespace Smurfs.UI.WPF;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashScreenWindow();
        splash.Show();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var connectionString = "Data Source=babuska.db";

                services.AddAppDataPersistence<DataContext>(options =>
                {
                    options.UseSqlite(connectionString);
                });

                services.AddAutoMapper(assemblies: typeof(MappingProfiles).Assembly);
                services.AddApplication();
                services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
                services.AddScoped<ISmurfReadService, SmurfReadService>();
                services.AddScoped<IPagingHandler<SmurfDto>, PagingHandler<SmurfDto>>();

                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssemblyContaining<List.Query>());

                // Register our ViewModel and View
                services.AddSingleton<MainWindowViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        Task.Run(async () =>
        {
            using var scope = _host.Services.CreateScope();

            await Current.Dispatcher.InvokeAsync(() =>
            {
                var scope = _host!.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                db.Database.MigrateAsync().GetAwaiter().GetResult();
                Seeding.SeedDatabase(db).GetAwaiter().GetResult();

                var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
                MainWindow = mainWindow;
                mainWindow.Show();
                splash.Close();
            });
        });
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
        }

        _host?.Dispose();
        base.OnExit(e);
    }
}
