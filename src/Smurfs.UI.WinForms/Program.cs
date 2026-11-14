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

namespace Smurfs.UI.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            using var logging = new FrontendFileLogging("winforms");
            using var host = StartHost(logging);
            using var form = new Form1(host.Services.GetRequiredService<MainWindowViewModel>());
            System.Windows.Forms.Application.Run(form);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Smurfs error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.ExitCode = 1;
        }
    }

    private static IHost StartHost(FrontendFileLogging logging)
    {
        IHost? host = null;
        try
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureLogging((context, builder) => logging.Configure(context.Configuration, builder))
                .ConfigureServices(services =>
                {
                    services.AddAppDataPersistence<DataContext>(options => options.UseSqlite("Data Source=babuska.db"));
                    services.AddApplication();
                    services.AddScoped<ISmurfReadService, SmurfReadService>();
                    services.AddSingleton<MainWindowViewModel>();
                })
                .Build();

            Task.Run(async () =>
            {
                await using var scope = host.Services.CreateAsyncScope();
                var initializer = scope.ServiceProvider.GetRequiredService<IAppDataInitializer>();
                await initializer.MigrateAsync();
                await initializer.SeedAsync();
            }).GetAwaiter().GetResult();

            return host;
        }
        catch (Exception exception)
        {
            logging.LogStartupFailure<Form1>(exception);
            host?.Dispose();
            throw;
        }
    }
}
