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
