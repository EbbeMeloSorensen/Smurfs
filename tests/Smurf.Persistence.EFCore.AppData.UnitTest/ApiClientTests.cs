using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Smurfs.Domain;
using Smurfs.Persistence.API;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies HTTP routes, authentication, page decoding, and safe failures without a running server.
/// </summary>
public sealed class ApiClientTests
{
    /// <summary>
    /// Login posts credentials once and subsequent requests attach a per-request bearer token.
    /// </summary>
    [Fact]
    public async Task LoginAndList_SendBearerAndPreserveFieldsAndServerPaging()
    {
        var id = Guid.NewGuid();
        var timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        using var transport = new StubHandler(async (request, call, _) =>
        {
            if (call == 1)
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("/api/account/login", request.RequestUri!.AbsolutePath);
                Assert.Null(request.Headers.Authorization);
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Assert.Equal("bob@test.com", body.RootElement.GetProperty("email").GetString());
                Assert.Equal("password", body.RootElement.GetProperty("password").GetString());
                return LoginResponse();
            }

            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/smurfs", request.RequestUri!.AbsolutePath);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization.Parameter);
            var query = Uri.UnescapeDataString(request.RequestUri.Query);
            Assert.Contains("pageNumber=2", query);
            Assert.Contains("pageSize=100", query);
            Assert.Contains("name=A&B +", query);
            Assert.Contains("infected=false|null", query);
            Assert.Contains("sorting=created", query);
            return PageResponse(new[] { new
            {
                id, name = "A&B +", createdUtc = timestamp, health = 42,
                birthday = (DateTime?)null, infected = (bool?)false, gender = "Female", petId = (Guid?)null
            } }, 2, 50, 51);
        });
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        var user = await api.LoginAsync("bob@test.com", "password");
        Assert.Equal("Bob", user.DisplayName);
        Assert.Equal("bob", user.Username);
        var page = await api.GetPageAsync(2, 100, CancellationToken.None, " A&B + ", [false, null], newestFirst: true);
        Assert.Equal(2, page.CurrentPage);
        Assert.Equal(50, page.PageSize);
        Assert.Equal(51, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
        var row = Assert.Single(page);
        Assert.Equal(id, row.Id);
        Assert.Equal(timestamp, row.CreatedUtc);
        Assert.Equal(DateTimeKind.Utc, row.CreatedUtc.Kind);
        Assert.Equal(42, row.Health);
        Assert.Null(row.Birthday);
        Assert.False(row.Infected);
        Assert.Equal(Gender.Female, row.Gender);
        Assert.Null(row.PetId);
        Assert.Null(http.DefaultRequestHeaders.Authorization);
    }

    /// <summary>
    /// A failed new login clears the old token and does not expose a server response body.
    /// </summary>
    [Fact]
    public async Task RejectedLogin_ClearsExistingSessionAndHidesBody()
    {
        using var transport = new StubHandler((_, call, _) => Task.FromResult(call == 1 ? LoginResponse() :
            new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("sensitive-server-body") }));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        var error = await Assert.ThrowsAsync<ApiException>(() => api.LoginAsync("email", "wrong"));
        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode);
        Assert.DoesNotContain("sensitive-server-body", error.ToString());
        await Assert.ThrowsAsync<ApiException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
        Assert.Equal(2, transport.Calls);
    }

    /// <summary>
    /// An expired token is discarded; a subsequent login restores access, including empty pages.
    /// </summary>
    [Fact]
    public async Task UnauthorizedRead_RequiresNewLogin()
    {
        using var transport = new StubHandler((_, call, _) => Task.FromResult(call switch
        {
            1 or 3 => LoginResponse(),
            2 => new HttpResponseMessage(HttpStatusCode.Unauthorized),
            _ => PageResponse(Array.Empty<object>(), 9, 5, 0)
        }));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        var error = await Assert.ThrowsAsync<ApiException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode);
        await Assert.ThrowsAsync<ApiException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
        Assert.Equal(2, transport.Calls);
        await api.LoginAsync("email", "password");
        var page = await api.GetPageAsync(9, 5, CancellationToken.None);
        Assert.Empty(page);
        Assert.Equal(9, page.CurrentPage);
        Assert.Equal(0, page.TotalPages);
    }

    /// <summary>
    /// Malformed login data never establishes an authenticated session.
    /// </summary>
    [Theory]
    [InlineData("null")]
    [InlineData("{broken")]
    [InlineData("{}")]
    [InlineData("{\"token\":\"\",\"displayName\":\"Bob\",\"username\":\"bob\"}")]
    public async Task InvalidLogin_IsRejected(string json)
    {
        using var transport = new StubHandler((_, _, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        }));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await Assert.ThrowsAsync<InvalidDataException>(() => api.LoginAsync("email", "password"));
        await Assert.ThrowsAsync<ApiException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
        Assert.Equal(1, transport.Calls);
    }

    /// <summary>
    /// Missing or inconsistent metadata is a protocol failure, not an invented row count.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"currentPage\":1,\"itemsPerPage\":0,\"totalItems\":0,\"totalPages\":0}")]
    [InlineData("{\"currentPage\":2,\"itemsPerPage\":5,\"totalItems\":0,\"totalPages\":0}")]
    public async Task InvalidPagination_IsRejected(string? header)
    {
        using var transport = new StubHandler((_, call, _) =>
        {
            var response = call == 1 ? LoginResponse() : new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<object>()) };
            if (call > 1 && header is not null)
            {
                response.Headers.TryAddWithoutValidation("Pagination", header);
            }

            return Task.FromResult(response);
        });
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        await Assert.ThrowsAsync<InvalidDataException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
    }

    /// <summary>
    /// HTTP failures preserve the status while suppressing arbitrary server error text.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Redirect)]
    public async Task HttpFailure_ReportsStatusWithoutResponseBody(HttpStatusCode status)
    {
        using var transport = new StubHandler((_, call, _) => Task.FromResult(call == 1 ? LoginResponse() :
            new HttpResponseMessage(status) { Content = new StringContent("private server details") }));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        var error = await Assert.ThrowsAsync<ApiException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
        Assert.Equal(status, error.StatusCode);
        Assert.DoesNotContain("private server details", error.ToString());
    }

    /// <summary>
    /// Cancellation reaches the transport, and invalid paging performs no HTTP request.
    /// </summary>
    [Fact]
    public async Task CancellationAndInvalidPaging_AreRespected()
    {
        using var transport = new StubHandler(async (_, call, token) =>
        {
            if (call == 1)
            {
                return LoginResponse();
            }

            await Task.Delay(Timeout.Infinite, token);
            throw new InvalidOperationException();
        });
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => api.GetPageAsync(0, 5, CancellationToken.None));
        Assert.Equal(1, transport.Calls);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => api.GetPageAsync(1, 5, timeout.Token));
    }

    /// <summary>
    /// Successful statuses still require a valid JSON array containing valid Smurf summaries.
    /// </summary>
    [Theory]
    [InlineData("null")]
    [InlineData("{broken")]
    [InlineData("[null]")]
    [InlineData("[{}]")]
    public async Task InvalidPageBody_IsRejected(string json)
    {
        using var transport = new StubHandler((_, call, _) =>
        {
            var response = call == 1 ? LoginResponse() : PageResponse(Array.Empty<object>(), 1, 5, 0);
            if (call > 1)
            {
                response.Content = new StringContent(json);
            }

            return Task.FromResult(response);
        });
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        await Assert.ThrowsAsync<InvalidDataException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
    }

    /// <summary>
    /// Name-only creation posts once with authentication, leaving identifiers and defaults to the server.
    /// </summary>
    [Fact]
    public async Task Create_PostsOnlyNameWithBearerWithoutRetrying()
    {
        using var transport = new StubHandler(async (request, call, _) =>
        {
            if (call == 1)
            {
                return LoginResponse();
            }

            Assert.Contains(request.Headers.Accept, item => item.MediaType == "application/json");
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/smurfs", request.RequestUri!.AbsolutePath);
            Assert.Equal("test-token", request.Headers.Authorization!.Parameter);
            Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            var field = Assert.Single(body.RootElement.EnumerateObject());
            Assert.Equal("name", field.Name);
            Assert.Equal("New Smurf", field.Value.GetString());
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        await api.CreateSmurfAsync("New Smurf");
        Assert.Equal(2, transport.Calls);
        Assert.Null(http.DefaultRequestHeaders.Authorization);
    }

    /// <summary>
    /// The two API validation shapes produce usable feedback, and the user can submit a corrected name.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_ReportsValidationAndAllowsCorrection(bool problemDetails)
    {
        using var transport = new StubHandler((_, call, _) => Task.FromResult(call switch
        {
            1 => LoginResponse(),
            2 => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = problemDetails
                    ? JsonContent.Create(new { errors = new { Name = new[] { "Name is required." } } })
                    : JsonContent.Create("Name is required.")
            },
            _ => new HttpResponseMessage(HttpStatusCode.OK)
        }));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        var error = await Assert.ThrowsAsync<ApiException>(() => api.CreateSmurfAsync(" "));
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Equal("Name is required.", error.Message);
        await api.CreateSmurfAsync("Corrected");
        Assert.Equal(3, transport.Calls);
    }

    /// <summary>
    /// Unexpected validation bodies use generic feedback instead of exposing raw server content.
    /// </summary>
    [Theory]
    [InlineData("<html>private details</html>")]
    [InlineData("null")]
    [InlineData("{\"detail\":\"private details\"}")]
    [InlineData("{\"errors\":{\"Name\":null}}")]
    public async Task Create_HandlesMalformedValidationSafely(string json)
    {
        using var transport = new StubHandler((_, call, _) => Task.FromResult(call == 1 ? LoginResponse() :
            new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(json) }));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        var error = await Assert.ThrowsAsync<ApiException>(() => api.CreateSmurfAsync("Invalid"));
        Assert.Equal("The Smurf was not created. Check the name and try again.", error.Message);
        Assert.DoesNotContain("private details", error.ToString());
        Assert.Equal(2, transport.Calls);
    }

    /// <summary>
    /// Creation requires sign-in, and a rejected token is discarded without replaying the operation.
    /// </summary>
    [Fact]
    public async Task Create_RequiresAuthenticationAndClearsRejectedToken()
    {
        using var transport = new StubHandler((_, call, _) => Task.FromResult(call == 1 ? LoginResponse() :
            new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await Assert.ThrowsAsync<ApiException>(() => api.CreateSmurfAsync("New"));
        Assert.Equal(0, transport.Calls);
        await api.LoginAsync("email", "password");
        var error = await Assert.ThrowsAsync<ApiException>(() => api.CreateSmurfAsync("New"));
        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode);
        await Assert.ThrowsAsync<ApiException>(() => api.GetPageAsync(1, 5, CancellationToken.None));
        await Assert.ThrowsAsync<ApiException>(() => api.CreateSmurfAsync("New"));
        Assert.Equal(2, transport.Calls);
    }

    /// <summary>
    /// Rejected or unconfirmed creates retain the status, hide server internals, and are not retried.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Create_ReportsHttpFailureWithoutRetrying(HttpStatusCode status)
    {
        using var transport = new StubHandler((_, call, _) => Task.FromResult(call == 1 ? LoginResponse() :
            new HttpResponseMessage(status) { Content = new StringContent("private server details") }));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        var error = await Assert.ThrowsAsync<ApiException>(() => api.CreateSmurfAsync("New"));
        Assert.Equal(status, error.StatusCode);
        Assert.DoesNotContain("private server details", error.ToString());
        if (status == HttpStatusCode.InternalServerError)
        {
            Assert.Contains("Check the list", error.Message);
        }

        Assert.Equal(2, transport.Calls);
    }

    /// <summary>
    /// A disconnected transport is propagated after one create attempt, never replayed.
    /// </summary>
    [Fact]
    public async Task Create_PropagatesTransportFailureWithoutRetrying()
    {
        using var transport = new StubHandler((_, call, _) => call == 1
            ? Task.FromResult(LoginResponse())
            : throw new HttpRequestException("Connection lost"));
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        await Assert.ThrowsAsync<HttpRequestException>(() => api.CreateSmurfAsync("New"));
        Assert.Equal(2, transport.Calls);
    }

    /// <summary>
    /// Cancellation reaches a pending create and does not trigger another submission.
    /// </summary>
    [Fact]
    public async Task Create_PropagatesCancellationWithoutRetrying()
    {
        using var transport = new StubHandler(async (_, call, token) =>
        {
            if (call == 1)
            {
                return LoginResponse();
            }

            await Task.Delay(Timeout.Infinite, token);
            throw new InvalidOperationException();
        });
        using var http = Client(transport);
        var api = new SmurfsApiClient(http);
        await api.LoginAsync("email", "password");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => api.CreateSmurfAsync("New", cancellation.Token));
        Assert.Equal(2, transport.Calls);
    }

    private static HttpClient Client(HttpMessageHandler handler) => new(handler)
    {
        BaseAddress = new Uri("https://localhost:7231/")
    };

    private static HttpResponseMessage LoginResponse() => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new { token = "test-token", displayName = "Bob", username = "bob" })
    };

    private static HttpResponseMessage PageResponse<T>(T[] items, int page, int size, int count)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(items) };
        response.Headers.Add("Pagination", JsonSerializer.Serialize(new
        {
            currentPage = page,
            itemsPerPage = size,
            totalItems = count,
            totalPages = (int) Math.Ceiling(count / (double) size)
        }));
        return response;
    }

    private sealed class StubHandler(Func<HttpRequestMessage, int, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => respond(request, ++Calls, cancellationToken);
    }
}
