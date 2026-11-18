using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Smurfs.Application.Interfaces;

namespace Smurfs.Infrastructure.Security;

/// <summary>
/// Reads the current account identity from the HTTP request.
/// </summary>
public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates the accessor using the current HTTP context provider.
    /// </summary>
    public UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Returns the current principal's name claim, or null if the request or claim is missing.
    /// </summary>
    public string? GetUsername()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
    }
}
