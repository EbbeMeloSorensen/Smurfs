namespace Smurfs.Application.Core;

/// <summary>
/// Represents the status and message of an error returned to an API client.
/// </summary>
public class AppException
{
    /// <summary>
    /// Creates an error response payload with a status code, message, and optional details.
    /// </summary>
    public AppException(int statusCode, string message, string details = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }

    /// <summary>
    /// Gets or sets the HTTP status code associated with the error.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message returned to the client.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets additional diagnostic details when available.
    /// </summary>
    public string Details { get; set; }
}
