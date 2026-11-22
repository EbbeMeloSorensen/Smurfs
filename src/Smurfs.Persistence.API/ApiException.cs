using System.Net;

namespace Smurfs.Persistence.API;

/// <summary>
/// Reports an unsuccessful HTTP status with safe feedback, including structured create-validation messages.
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
