using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommandLine;
using Smurfs.Persistence.API;
using Smurfs.UI.Console.API;
using Smurfs.UI.Console.API.Authentication;
using Smurfs.UI.Console.API.Verbs;
using CreateOptions = Smurfs.UI.Console.API.Verbs.Create;
using ListOptions = Smurfs.UI.Console.API.Verbs.List;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies separate CLI operations, secure-token lifecycle boundaries, and non-interactive execution.
/// </summary>
public sealed class ApiConsoleTests
{
    private static readonly Uri Address = new("https://localhost:7231/");

    /// <summary>
    /// A login saves a token once; independent command objects reuse it without another login.
    /// </summary>
    [Fact]
    public async Task SavedLogin_IsReusedByIndependentCommands()
    {
        var store = new MemoryTokenStore();
        using var handler = new StubHandler(async request =>
        {
            if (request.RequestUri!.AbsolutePath == "/api/account/login")
            {
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Assert.Equal("password", body.RootElement.GetProperty("password").GetString());
                return LoginResponse();
            }

            Assert.Equal("saved-token", request.Headers.Authorization!.Parameter);
            return PageResponse();
        });
        using var loginHttp = Http(handler);
        var login = Console(loginHttp, store, "password\n");
        Assert.Equal(0, await login.App.LoginAsync(new Login { Email = "bob@test.com", PasswordStdin = true }));
        Assert.Equal("saved-token", store.Read(Address));
        Assert.DoesNotContain("saved-token", login.Output.ToString() + login.Error);
        using var listHttp = Http(handler);
        var list = Console(listHttp, store);
        Assert.Equal(0, await list.App.ListAsync(new ListOptions { Page = 1, PageSize = 25 }));
        Assert.Equal("Smurfs (page 1, 1 total pages, 1 rows):\n Original\n", list.Output.ToString().Replace("\r\n", "\n"));
        Assert.Equal(2, handler.Requests);
    }

    /// <summary>
    /// Interactive login prompts once, hides password entry through its reader, and does not start a menu.
    /// </summary>
    [Fact]
    public async Task InteractiveLogin_PerformsOneAuthenticationOnly()
    {
        using var handler = new StubHandler(_ => Task.FromResult(LoginResponse()));
        using var http = Http(handler);
        var output = new StringWriter();
        var error = new StringWriter();
        var store = new MemoryTokenStore();
        var passwordReads = 0;
        var app = new ApiConsole(new SmurfsApiClient(http), Address, store, new StringReader("bob@test.com\n"), output, error,
            () => { passwordReads++; return "password"; }, false, null);
        Assert.Equal(0, await app.LoginAsync(new Login()));
        Assert.Equal(1, passwordReads);
        Assert.Contains("Email:", error.ToString());
        Assert.Contains("Password:", error.ToString());
        Assert.DoesNotContain("password", output.ToString() + error);
        Assert.Equal(1, handler.Requests);
        Assert.Equal(1, store.Writes);
    }

    /// <summary>
    /// No saved credentials means failure without a prompt or HTTP request.
    /// </summary>
    [Fact]
    public async Task MissingLogin_FailsWithoutPromptOrNetwork()
    {
        using var handler = new StubHandler(_ => throw new InvalidOperationException("No HTTP expected"));
        using var http = Http(handler);
        var run = Console(http, new MemoryTokenStore());
        Assert.Equal(1, await run.App.ListAsync(new ListOptions { Page = 1, PageSize = 25 }));
        Assert.Equal(1, await run.App.CreateAsync(new CreateOptions { Name = "New" }));
        Assert.Contains("Run login", run.Error.ToString());
        Assert.Equal("", run.Output.ToString());
        Assert.Equal(0, handler.Requests);
    }

    /// <summary>
    /// Environment authentication takes precedence without consulting or changing the credential store.
    /// </summary>
    [Fact]
    public async Task EnvironmentToken_BypassesSavedCredentials()
    {
        var store = new MemoryTokenStore { FailReads = true };
        using var handler = new StubHandler(request =>
        {
            Assert.Equal("environment-token", request.Headers.Authorization!.Parameter);
            return Task.FromResult(PageResponse());
        });
        using var http = Http(handler);
        var run = Console(http, store, environmentToken: "environment-token");
        Assert.Equal(0, await run.App.ListAsync(new ListOptions { Page = 1, PageSize = 25 }));
        Assert.Equal(0, store.Writes);
        Assert.Equal(0, store.Deletes);
    }

    /// <summary>
    /// A rejected token exits without login prompts, fallback authentication, or automatic replay.
    /// </summary>
    [Fact]
    public async Task RejectedToken_FailsOnceWithoutReplayOrCredentialMutation()
    {
        var store = new MemoryTokenStore();
        store.Save(Address, "saved-token");
        using var handler = new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        using var http = Http(handler);
        var run = Console(http, store);
        Assert.Equal(1, await run.App.CreateAsync(new CreateOptions { Name = "New" }));
        Assert.Contains("Run login again", run.Error.ToString());
        Assert.Equal(1, handler.Requests);
        Assert.Equal(0, store.Deletes);
        Assert.Equal("saved-token", store.Read(Address));
        Assert.Equal("", run.Output.ToString());
    }

    /// <summary>
    /// Creation prints the original console's confirmation and exits without fetching a list.
    /// </summary>
    [Fact]
    public async Task Create_PerformsOneOperationAndMatchesOriginalOutput()
    {
        using var handler = new StubHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/smurfs", request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        using var http = Http(handler);
        var run = Console(http, new MemoryTokenStore(), environmentToken: "environment-token");
        Assert.Equal(0, await run.App.CreateAsync(new CreateOptions { Name = "Kylling" }));
        Assert.Equal("Smurf: \"Kylling\" created successfully.", run.Output.ToString().Trim());
        Assert.Equal(1, handler.Requests);
        Assert.Equal("", run.Error.ToString());
    }

    /// <summary>
    /// Lost create responses return a failure status without false success or another request.
    /// </summary>
    [Fact]
    public async Task UncertainCreate_ReturnsFailureWithoutRetryOrListReload()
    {
        using var handler = new StubHandler(_ => throw new HttpRequestException("Connection lost"));
        using var http = Http(handler);
        var run = Console(http, new MemoryTokenStore(), environmentToken: "environment-token");
        Assert.Equal(1, await run.App.CreateAsync(new CreateOptions { Name = "New" }));
        Assert.Contains("outcome is unknown", run.Error.ToString());
        Assert.Equal("", run.Output.ToString());
        Assert.Equal(1, handler.Requests);
    }

    /// <summary>
    /// Failed sign-in cannot replace a previously saved token or report success.
    /// </summary>
    [Fact]
    public async Task FailedLogin_PreservesExistingSavedToken()
    {
        var store = new MemoryTokenStore();
        store.Save(Address, "existing-token");
        using var handler = new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        using var http = Http(handler);
        var run = Console(http, store, "wrong\n");
        await Assert.ThrowsAsync<ApiException>(() => run.App.LoginAsync(new Login { Email = "bob@test.com", PasswordStdin = true }));
        Assert.Equal("existing-token", store.Read(Address));
        Assert.Equal("", run.Output.ToString());
    }

    /// <summary>
    /// Redirected login never prompts unless the explicit stdin-password option is supplied.
    /// </summary>
    [Fact]
    public async Task Login_RejectsImplicitRedirectedPromptsAndMissingEmail()
    {
        using var handler = new StubHandler(_ => throw new InvalidOperationException("No HTTP expected"));
        using var http = Http(handler);
        var run = Console(http, new MemoryTokenStore());
        Assert.Equal(1, await run.App.LoginAsync(new Login()));
        Assert.Equal(2, await run.App.LoginAsync(new Login { PasswordStdin = true }));
        Assert.Equal(0, handler.Requests);
    }

    /// <summary>
    /// Logout removes only the selected endpoint's token and explains active environment overrides.
    /// </summary>
    [Fact]
    public async Task Logout_IsEndpointScopedAndDoesNotClaimEnvironmentRevocation()
    {
        var other = new Uri("https://localhost:7443/");
        var store = new MemoryTokenStore();
        store.Save(Address, "one");
        store.Save(other, "two");
        using var handler = new StubHandler(_ => throw new InvalidOperationException("No HTTP expected"));
        using var http = Http(handler);
        var run = Console(http, store, environmentToken: "override");
        Assert.Equal(0, await run.App.LogoutAsync());
        Assert.Null(store.Read(Address));
        Assert.Equal("two", store.Read(other));
        Assert.Contains("remains active", run.Error.ToString());
        Assert.Equal(0, handler.Requests);
    }

    /// <summary>
    /// API command spellings and page/name options match the existing direct-persistence CLI.
    /// </summary>
    [Fact]
    public void Parsing_AcceptsOriginalCreateAndListOptions()
    {
        using var parser = new Parser(options => options.HelpWriter = null);
        var create = parser.ParseArguments<CreateOptions, ListOptions, Login, Logout>(["createsmurf", "--name", "Kylling"]);
        var createValue = Assert.IsType<CreateOptions>(Assert.IsType<Parsed<object>>(create).Value);
        Assert.Equal("Kylling", createValue.Name);
        var list = parser.ParseArguments<CreateOptions, ListOptions, Login, Logout>(["listsmurfs", "--page", "2", "--page-size", "5"]);
        var listValue = Assert.IsType<ListOptions>(Assert.IsType<Parsed<object>>(list).Value);
        Assert.Equal(2, listValue.Page);
        Assert.Equal(5, listValue.PageSize);
    }

    /// <summary>
    /// Credential targets normalize equivalent hosts and keep different scheme/port/path endpoints isolated.
    /// </summary>
    [Fact]
    public void CredentialTargets_AreCanonicalAndEndpointScoped()
    {
        var target = WindowsTokenStore.GetTarget(new Uri("https://EXAMPLE.com:443/"));
        Assert.Equal(target, WindowsTokenStore.GetTarget(new Uri("https://example.com/")));
        foreach (var other in new[] { "http://example.com/", "https://example.com:444/", "https://example.com/sub/" })
        {
            Assert.NotEqual(target, WindowsTokenStore.GetTarget(new Uri(other)));
        }
    }

    /// <summary>
    /// Exercises the real Windows credential API using a unique test-only entry and always removes it.
    /// </summary>
    [Fact]
    public void WindowsCredentialStore_RoundTripsAcrossInstances()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Native integration applies only on Windows; environment-token tests are portable.
        }

        var address = new Uri($"https://smurfs-test-{Guid.NewGuid():N}.invalid/");
        var store = new WindowsTokenStore();
        try
        {
            Assert.Null(store.Read(address));
            store.Save(address, "test-placeholder-token");
            Assert.Equal("test-placeholder-token", new WindowsTokenStore().Read(address));
            store.Save(address, "replacement-token");
            Assert.Equal("replacement-token", new WindowsTokenStore().Read(address));
            store.Delete(address);
            Assert.Null(new WindowsTokenStore().Read(address));
            store.Delete(address);
        }
        finally
        {
            store.Delete(address);
        }
    }

    private static (ApiConsole App, StringWriter Output, StringWriter Error) Console(
        HttpClient http, ITokenStore store, string input = "", string? environmentToken = null)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var app = new ApiConsole(new SmurfsApiClient(http), Address, store, new StringReader(input), output, error,
            () => throw new InvalidOperationException("Unexpected password prompt"), true, environmentToken);
        return (app, output, error);
    }

    private static HttpClient Http(HttpMessageHandler handler) => new(handler, disposeHandler: false) { BaseAddress = Address };

    private static HttpResponseMessage LoginResponse() => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new { token = "saved-token", displayName = "Bob", username = "bob" })
    };

    private static HttpResponseMessage PageResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[] { new { id = Guid.NewGuid(), name = "Original", createdUtc = DateTime.UtcNow } })
        };
        response.Headers.Add("Pagination", "{\"currentPage\":1,\"itemsPerPage\":25,\"totalItems\":1,\"totalPages\":1}");
        return response;
    }

    private sealed class MemoryTokenStore : ITokenStore
    {
        private readonly Dictionary<Uri, string> _tokens = [];

        public bool FailReads { get; init; }

        public int Writes { get; private set; }

        public int Deletes { get; private set; }

        public string? Read(Uri address) => FailReads ? throw new InvalidOperationException("Credential lookup not expected") : _tokens.GetValueOrDefault(address);

        public void Save(Uri address, string token)
        {
            _tokens[address] = token;
            Writes++;
        }

        public void Delete(Uri address)
        {
            _tokens.Remove(address);
            Deletes++;
        }
    }

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            return respond(request);
        }
    }
}
