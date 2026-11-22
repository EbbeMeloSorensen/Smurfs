using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Smurfs.Application.Smurfs;
using Smurfs.Persistence.API;

namespace Smurfs.UI.Console.API;

internal static class Program
{
    private static async Task<int> Main()
    {
        try
        {
            using var configuration = new ConfigurationManager();

            configuration.SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables();

            var baseUrl = configuration["Api:BaseUrl"];

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var address))
            {
                throw new InvalidOperationException("Configure Api:BaseUrl as an absolute HTTP(S) server URL ending in /.");
            }

            using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
            {
                BaseAddress = address,
                Timeout = TimeSpan.FromSeconds(30)
            };

            var api = new SmurfsApiClient(http);
            System.Console.WriteLine($"Smurfs API console — {address}");
            return await RunAsync(api);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or ArgumentException or InvalidOperationException or OperationCanceledException)
        {
            System.Console.Error.WriteLine(exception is HttpRequestException and not ApiException
                ? "Could not contact the API. Check its address, availability, and HTTPS certificate."
                : exception is OperationCanceledException ? "The API request timed out or was cancelled." : exception.Message);
            return 1;
        }
    }

    private static async Task<int> RunAsync(SmurfsApiClient api)
    {
        var handler = new List.Handler(api);

        while (true)
        {
            System.Console.Write("Email (blank to quit): ");
            var email = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(email))
            {
                return 0;
            }

            System.Console.Write("Password: ");
            var password = ReadPassword();

            if (password is null)
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                System.Console.Error.WriteLine("Enter a password.");
                continue;
            }

            try
            {
                var user = await api.LoginAsync(email.Trim(), password);
                System.Console.WriteLine($"Signed in as {user.DisplayName} ({user.Username}).");
            }
            catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
            {
                System.Console.Error.WriteLine(exception.Message);
                continue;
            }

            var pageNumber = 1;
            var pageSize = 5;

            while (true)
            {
                try
                {
                    var result = await handler.Handle(new List.Query
                    {
                        Params = new SmurfParams { PageNumber = pageNumber, PageSize = pageSize }
                    }, CancellationToken.None);
                    if (!result.IsSuccess)
                    {
                        System.Console.Error.WriteLine(result.Error);
                        return 1;
                    }

                    var page = result.Value;
                    System.Console.WriteLine($"Smurfs (page {page.CurrentPage}, {page.TotalPages} total pages, {page.TotalCount} rows):");

                    foreach (var smurf in page)
                    {
                        System.Console.WriteLine($" {smurf.Name}");
                    }

                    System.Console.Write("Next / Previous / Reload / Login / Quit (n/p/r/l/q): ");

                    var command = System.Console.ReadLine()?.Trim().ToLowerInvariant();

                    if (command is null or "q" or "quit")
                    {
                        return 0;
                    }

                    if (command is "l" or "login")
                    {
                        break;
                    }

                    if (command is "n" or "next")
                    {
                        if (pageNumber < page.TotalPages)
                        {
                            pageNumber++;
                        }
                        else
                        {
                            System.Console.WriteLine("Already at the last page.");
                        }
                    }
                    else if (command is "p" or "previous")
                    {
                        if (pageNumber > 1)
                        {
                            pageNumber--;
                        }
                        else
                        {
                            System.Console.WriteLine("Already at the first page.");
                        }
                    }
                    else if (command is not ("r" or "reload"))
                    {
                        System.Console.WriteLine("Use n, p, r, l, or q.");
                    }
                }
                catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
                {
                    System.Console.Error.WriteLine(exception.Message);
                    break;
                }
            }
        }
    }

    private static string? ReadPassword()
    {
        if (System.Console.IsInputRedirected)
        {
            return System.Console.ReadLine();
        }

        var password = new StringBuilder();
        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                System.Console.WriteLine();
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
