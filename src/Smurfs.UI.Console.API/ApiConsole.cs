using System.Net;
using Smurfs.Application.Smurfs;
using Smurfs.Persistence.API;
using Smurfs.UI.Console.API.Authentication;
using Smurfs.UI.Console.API.Verbs;
using CreateOptions = Smurfs.UI.Console.API.Verbs.Create;
using ListUseCase = Smurfs.Application.Smurfs.List;
using ListOptions = Smurfs.UI.Console.API.Verbs.List;

namespace Smurfs.UI.Console.API;

internal sealed class ApiConsole(
    SmurfsApiClient api,
    Uri address,
    ITokenStore tokenStore,
    TextReader input,
    TextWriter output,
    TextWriter error,
    Func<string?> readPassword,
    bool inputRedirected,
    string? environmentToken)
{
    public async Task<int> LoginAsync(Login options)
    {
        if (options.PasswordStdin && string.IsNullOrWhiteSpace(options.Email))
        {
            await error.WriteLineAsync("--password-stdin requires --email.");
            return 2;
        }

        if (!options.PasswordStdin && inputRedirected)
        {
            await error.WriteLineAsync("Login requires interactive input, or --email with --password-stdin for scripts.");
            return 1;
        }

        var email = options.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            await error.WriteAsync("Email: ");
            email = await input.ReadLineAsync();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            await error.WriteLineAsync("Email is required.");
            return 1;
        }

        if (!options.PasswordStdin)
        {
            await error.WriteAsync("Password: ");
        }

        var password = options.PasswordStdin ? await input.ReadLineAsync() : readPassword();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await error.WriteLineAsync("Email and password are required.");
            return 1;
        }

        var user = await api.LoginAsync(email.Trim(), password);
        tokenStore.Save(address, api.GetAccessToken());
        await output.WriteLineAsync($"Signed in as {user.DisplayName} ({user.Username}).");
        if (!string.IsNullOrWhiteSpace(environmentToken))
        {
            await error.WriteLineAsync("Login saved. Commands currently use the SMURFS_API_TOKEN override.");
        }

        return 0;
    }

    public async Task<int> LogoutAsync()
    {
        tokenStore.Delete(address);
        await output.WriteLineAsync("Saved login removed for this API address.");
        if (!string.IsNullOrWhiteSpace(environmentToken))
        {
            await error.WriteLineAsync("SMURFS_API_TOKEN remains active until you unset it in your shell or script.");
        }

        return 0;
    }

    public Task<int> ListAsync(ListOptions options) => RunAuthenticatedAsync(async () =>
    {
        var result = await new ListUseCase.Handler(api).Handle(new ListUseCase.Query
        {
            Params = new SmurfParams { PageNumber = options.Page, PageSize = options.PageSize }
        }, CancellationToken.None);
        if (!result.IsSuccess)
        {
            await error.WriteLineAsync(result.Error);
            return 1;
        }

        var page = result.Value;
        await output.WriteLineAsync($"Smurfs (page {page.CurrentPage}, {page.TotalPages} total pages, {page.TotalCount} rows):");
        foreach (var smurf in page)
        {
            await output.WriteLineAsync($" {smurf.Name}");
        }

        return 0;
    });

    public Task<int> CreateAsync(CreateOptions options) => RunAuthenticatedAsync(async () =>
    {
        try
        {
            await api.CreateSmurfAsync(options.Name);
        }
        catch (Exception exception) when (exception is not ApiException &&
            exception is HttpRequestException or IOException or OperationCanceledException)
        {
            await error.WriteLineAsync("Creation outcome is unknown. Check the list before trying again; the request was not retried.");
            return 1;
        }

        await output.WriteLineAsync($"Smurf: \"{options.Name}\" created successfully.");
        return 0;
    });

    private async Task<int> RunAuthenticatedAsync(Func<Task<int>> operation)
    {
        var token = string.IsNullOrWhiteSpace(environmentToken) ? tokenStore.Read(address) : environmentToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            await error.WriteLineAsync("Not signed in. Run login first, or supply SMURFS_API_TOKEN.");
            return 1;
        }

        api.UseAccessToken(token);
        try
        {
            return await operation();
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            await error.WriteLineAsync("Authentication expired or was rejected. Run login again, or replace SMURFS_API_TOKEN if you use it.");
            return 1;
        }
    }
}
