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
    /// Reads the account username from the current principal's name claim.
    /// </summary>
    public string GetUsername()
    {
        return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
    }
}
