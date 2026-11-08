using System.Windows;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Domain.BusinessRules;
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

    /// <summary>
    /// Prepares the database before replacing the splash screen with the main window.
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var shutdownMode = ShutdownMode;
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var splash = new SplashScreenWindow();

        try
        {
            splash.Show();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    const string connectionString = "Data Source=babuska.db";

                    services.AddAppDataPersistence<DataContext>(options => options.UseSqlite(connectionString));
                    services.AddAutoMapper(assemblies: typeof(MappingProfiles).Assembly);
                    services.AddApplication();
                    services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
                    services.AddScoped<ISmurfReadService, SmurfReadService>();
                    services.AddScoped<IPagingHandler<SmurfDto>, PagingHandler<SmurfDto>>();
                    services.AddScoped<IBusinessRuleCatalog, BusinessRuleCatalog>();
                    services.AddTransient<CreateSmurfDialogViewModel>();
                    services.AddTransient<CreateSmurfDialog>();
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .Build();
            _host = host;

            await Task.Run(async () =>
            {
                await using var scope = host.Services.CreateAsyncScope();
                var initializer = scope.ServiceProvider.GetRequiredService<IAppDataInitializer>();
                await initializer.MigrateAsync();
                await initializer.SeedAsync();
            });

            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
            ShutdownMode = shutdownMode;
            splash.Close();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"Smurfs could not start. {exception.Message}",
                "Smurfs startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <summary>
    /// Disposes the host and its service provider when WPF exits.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _host?.Dispose();
        }
        finally
        {
            base.OnExit(e);
        }
    }
}
