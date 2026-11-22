using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Domain;

namespace Smurfs.Persistence.API;

/// <summary>
/// Signs in to Smurfs.API and implements its application read contract over HTTP.
/// </summary>
/// <remarks>
/// Owns an in-memory token for one user session. Do not share instances between users or use them concurrently.
/// The caller owns HttpClient and should disable redirects and cookies; no certificate validation is bypassed.
/// No repository, write unit of work, storage initializer, or token persistence is provided by this milestone.
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
    /// Clears any previous token, posts credentials, and retains the new token only after a valid response.
    /// Invalid credentials throw ApiException with status 401. Passwords and tokens are never logged or saved.
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

        _token = user.Token;
        return new ApiUser { DisplayName = user.DisplayName, Username = user.Username };
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
