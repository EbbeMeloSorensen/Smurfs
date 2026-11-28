using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Domain.VillageLife;

namespace Smurfs.Persistence.API;

/// <summary>
/// Signs in, creates named Smurfs, and implements the application read contract over HTTP.
/// </summary>
/// <remarks>
/// Owns an in-memory token for one user session. Do not share instances between users or use them concurrently.
/// The caller owns HttpClient and should disable redirects and cookies; no certificate validation is bypassed.
/// No repository, write unit of work, or storage initializer is provided. Token persistence is owned by the caller.
/// </remarks>
public sealed class SmurfsApiClient : ISmurfReadService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter<Gender>(allowIntegerValues: false) }
    };

    private readonly HttpClient _httpClient;

    private string? _token;

    /// <summary>
    /// Uses an externally owned HTTP client whose absolute HTTP(S) BaseAddress ends in a slash.
    /// Relative routes include api/, so BaseAddress normally identifies the server root.
    /// </summary>
    public SmurfsApiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        var address = httpClient.BaseAddress;
        if (address is null || (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps) ||
            !address.AbsolutePath.EndsWith('/') || address.Query.Length != 0 || address.Fragment.Length != 0 || address.UserInfo.Length != 0)
        {
            throw new ArgumentException("Configure an absolute HTTP(S) API base URL ending in / without credentials, query, or fragment.", nameof(httpClient));
        }

        _httpClient = httpClient;
    }

    /// <summary>
    /// Supplies an existing bearer token from the caller's credential store or environment.
    /// The server validates it on the next request; this method does not authenticate or refresh it.
    /// </summary>
    public void UseAccessToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        if (token.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)))
        {
            throw new ArgumentException("The API token contains invalid characters.", nameof(token));
        }

        _token = token;
    }

    /// <summary>
    /// Exports the current token explicitly for secure storage by the caller.
    /// Treat the returned value as a secret; never print or log it.
    /// </summary>
    public string GetAccessToken() => _token ?? throw new InvalidOperationException("No API access token is available.");

    /// <summary>
    /// Clears any previous token, posts credentials, and retains the new token only after a valid response.
    /// Invalid credentials throw ApiException with status 401. Passwords are never retained. The token remains in memory until explicitly exported by the caller.
    /// </summary>
    public async Task<ApiUser> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        _token = null;
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        using var response = await _httpClient.PostAsJsonAsync("api/account/login", new { email, password }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(response.StatusCode, response.StatusCode == HttpStatusCode.Unauthorized
                ? "Sign-in failed. Check your email and password."
                : $"Sign-in failed (HTTP {(int) response.StatusCode}).");
        }

        var user = await ReadJsonAsync<LoginResponse>(response, cancellationToken);
        if (string.IsNullOrWhiteSpace(user.Token) || string.IsNullOrWhiteSpace(user.DisplayName) || string.IsNullOrWhiteSpace(user.Username))
        {
            throw new InvalidDataException("The API returned an incomplete sign-in response.");
        }

        UseAccessToken(user.Token);
        return new ApiUser { DisplayName = user.DisplayName, Username = user.Username };
    }

    /// <summary>
    /// Sends a name-only create operation to the server, which validates and commits it.
    /// Health defaults to zero and optional values to null. No client identifier is generated.
    /// </summary>
    /// <remarks>
    /// Requires sign-in. A 400 exposes structured validation feedback; a 401 clears the session.
    /// Requests are never retried. A transport failure or timeout can occur after the server committed;
    /// callers must report that uncertain outcome and check the list before submitting again.
    /// </remarks>
    public async Task CreateSmurfAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (_token is null)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "Sign in before creating a Smurf.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/smurfs")
        {
            Content = JsonContent.Create(new { name })
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _token = null;
            throw new ApiException(response.StatusCode, "Authentication expired or was rejected. Sign in again.");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new ApiException(response.StatusCode, await ReadCreateValidationAsync(response, cancellationToken));
        }

        throw new ApiException(response.StatusCode, response.StatusCode == HttpStatusCode.Forbidden
            ? "You are not allowed to create a Smurf."
            : $"Creation was not confirmed (HTTP {(int) response.StatusCode}). Check the list before trying again.");
    }

    /// <inheritdoc />
    public async Task<PagedList<SmurfDto>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken,
        string? name = null,
        IReadOnlyCollection<bool?>? infected = null,
        bool newestFirst = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        if (_token is null)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "Sign in before loading Smurfs.");
        }

        var query = $"api/smurfs?pageNumber={pageNumber.ToString(CultureInfo.InvariantCulture)}&pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}";
        if (!string.IsNullOrWhiteSpace(name))
        {
            query += "&name=" + Uri.EscapeDataString(name.Trim());
        }

        if (infected is { Count: > 0 })
        {
            var states = string.Join('|', infected.Select(value => value is null ? "null" : value.Value ? "true" : "false"));
            query += "&infected=" + Uri.EscapeDataString(states);
        }

        query += "&sorting=" + (newestFirst ? "created" : "name");
        using var request = new HttpRequestMessage(HttpMethod.Get, query);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _token = null;
            }

            throw new ApiException(response.StatusCode, response.StatusCode == HttpStatusCode.Unauthorized
                ? "Authentication expired or was rejected. Sign in again."
                : $"Loading Smurfs failed (HTTP {(int) response.StatusCode}).");
        }

        if (!response.Headers.TryGetValues("Pagination", out var headers))
        {
            throw new InvalidDataException("The API response is missing its Pagination header.");
        }

        Pagination pagination;
        try
        {
            var values = headers.ToArray();
            pagination = values.Length == 1
                ? JsonSerializer.Deserialize<Pagination>(values[0], JsonOptions) ?? throw new JsonException()
                : throw new JsonException();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The API returned an invalid Pagination header.", exception);
        }

        if (pagination.CurrentPage != pageNumber || pagination.ItemsPerPage < 1 || pagination.ItemsPerPage > pageSize ||
            pagination.TotalItems < 0 || pagination.TotalPages != (int) Math.Ceiling(pagination.TotalItems / (double) pagination.ItemsPerPage))
        {
            throw new InvalidDataException("The API returned inconsistent pagination metadata.");
        }

        var items = await ReadJsonAsync<List<SmurfDto>>(response, cancellationToken);
        if (items.Count > pagination.ItemsPerPage || items.Any(item => item is null || item.Id == Guid.Empty || string.IsNullOrWhiteSpace(item.Name)))
        {
            throw new InvalidDataException("The API returned an invalid Smurf page.");
        }

        return new PagedList<SmurfDto>(items, pagination.TotalItems, pagination.CurrentPage, pagination.ItemsPerPage);
    }

    private static async Task<string> ReadCreateValidationAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        const string fallback = "The Smurf was not created. Check the name and try again.";
        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.String)
            {
                var message = root.GetString();
                return string.IsNullOrWhiteSpace(message) ? fallback : message;
            }

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in errors.EnumerateObject())
                {
                    if (field.Value.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var error in field.Value.EnumerateArray())
                    {
                        if (error.ValueKind == JsonValueKind.String)
                        {
                            var message = error.GetString();
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                return message;
                            }
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Only the API's validation string or problem-details errors are user-facing feedback.
        }

        return fallback;
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
                ?? throw new InvalidDataException("The API returned an empty JSON response.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The API returned an invalid JSON response.", exception);
        }
    }

    private sealed class LoginResponse
    {
        public required string Token { get; init; }

        public required string DisplayName { get; init; }

        public required string Username { get; init; }
    }

    private sealed class Pagination
    {
        public required int CurrentPage { get; init; }

        public required int ItemsPerPage { get; init; }

        public required int TotalItems { get; init; }

        public required int TotalPages { get; init; }
    }
}
