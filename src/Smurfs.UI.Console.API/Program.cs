using System.ComponentModel;
using System.Text;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Smurfs.Persistence.API;
using Smurfs.UI.Console.API.Authentication;
using Smurfs.UI.Console.API.Verbs;
using CreateOptions = Smurfs.UI.Console.API.Verbs.Create;
using ListOptions = Smurfs.UI.Console.API.Verbs.List;

namespace Smurfs.UI.Console.API;

internal static class Program
{
    private static Task<int> Main(string[] args) =>
        Parser.Default.ParseArguments<Login, Logout, CreateOptions, ListOptions>(args)
            .MapResult(
                (Login options) => RunAsync(console => console.LoginAsync(options)),
                (Logout _) => RunAsync(console => console.LogoutAsync()),
                (CreateOptions options) => RunAsync(console => console.CreateAsync(options)),
                (ListOptions options) => RunAsync(console => console.ListAsync(options)),
                errors => Task.FromResult(errors.All(error => error.StopsProcessing) ? 0 : 2));

    private static async Task<int> RunAsync(Func<ApiConsole, Task<int>> operation)
    {
        try
        {
            using var configuration = new ConfigurationManager();
            configuration.SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables();
            if (!Uri.TryCreate(configuration["Api:BaseUrl"], UriKind.Absolute, out var address))
            {
                throw new InvalidOperationException("Configure Api:BaseUrl as an absolute HTTP(S) server URL ending in /.");
            }

            using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
            {
                BaseAddress = address,
                Timeout = TimeSpan.FromSeconds(30)
            };
            var api = new SmurfsApiClient(http);
            var console = new ApiConsole(api, address, new PlatformTokenStore(), System.Console.In,
                System.Console.Out, System.Console.Error, ReadPassword, System.Console.IsInputRedirected,
                Environment.GetEnvironmentVariable("SMURFS_API_TOKEN"));
            return await operation(console);
        }
        catch (Win32Exception exception)
        {
            System.Console.Error.WriteLine($"Could not access Windows Credential Manager (error {exception.NativeErrorCode}).");
            return 1;
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or UnauthorizedAccessException or ArgumentException or
            InvalidOperationException or OperationCanceledException or PlatformNotSupportedException)
        {
            System.Console.Error.WriteLine(exception is HttpRequestException and not ApiException
                ? "Could not contact the API. Check its address, availability, and HTTPS certificate."
                : exception is OperationCanceledException ? "The API request timed out or was cancelled." : exception.Message);
            return 1;
        }
    }

    private static string? ReadPassword()
    {
        var password = new StringBuilder();
        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                System.Console.Error.WriteLine();
                return password.ToString();
            }

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Length--;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
            }
        }
    }
}
