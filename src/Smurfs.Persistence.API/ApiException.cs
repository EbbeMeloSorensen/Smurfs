using System.Net;

namespace Smurfs.Persistence.API;

/// <summary>
/// Reports an unsuccessful HTTP status without including response bodies or credentials.
/// </summary>
public sealed class ApiException : HttpRequestException
{
    /// <summary>
    /// Creates a client-facing failure with its HTTP status.
    /// </summary>
    public ApiException(HttpStatusCode statusCode, string message) : base(message, null, statusCode)
    {
    }
}
