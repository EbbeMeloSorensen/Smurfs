using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Infrastructure.Logging;
using Smurfs.Infrastructure.Smurfs;
using Smurfs.Persistence;
using Smurfs.Persistence.EFCore.AppData;
using Smurfs.ViewModel;

namespace Smurfs.UI.Avalonia;

/// <summary>
/// Composes the Avalonia frontend and owns its application host.
/// </summary>
public partial class App : global::Avalonia.Application
{
    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var fileLogging = new FrontendFileLogging("avalonia");
            IHost? host = null;

            try
            {
                host = Host.CreateDefaultBuilder()
                    .ConfigureLogging((context, logging) => fileLogging.Configure(context.Configuration, logging))
                    .ConfigureServices(services =>
                    {
                        services.AddAppDataPersistence<DataContext>(options => options.UseSqlite("Data Source=babuska.db"));
                        services.AddApplication();
                        services.AddScoped<ISmurfReadService, SmurfReadService>();
                        services.AddSingleton<MainWindowViewModel>();
                    })
                    .Build();

                InitializeDatabaseAsync(host.Services).GetAwaiter().GetResult();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = host.Services.GetRequiredService<MainWindowViewModel>()
                };
            }
            catch (Exception exception)
            {
                fileLogging.LogStartupFailure<App>(exception);
                desktop.MainWindow = new Window
                {
                    Title = "Smurfs startup error",
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Content = new TextBlock
                    {
                        Margin = new Thickness(16),
                        MaxWidth = 520,
                        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                        Text = $"Smurfs could not start. {exception.Message}"
                    }
                };
            }

            desktop.Exit += (_, _) =>
            {
                try
                {
                    host?.Dispose();
                }
                finally
                {
                    fileLogging.Dispose();
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static Task InitializeDatabaseAsync(IServiceProvider services) => Task.Run(async () =>
    {
        await using var scope = services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IAppDataInitializer>();
        await initializer.MigrateAsync();
        await initializer.SeedAsync();
    });
}
