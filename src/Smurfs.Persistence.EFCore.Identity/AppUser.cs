using Microsoft.AspNetCore.Identity;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Extends the Identity user with a display name.
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the name displayed for the user.
    /// </summary>
    public string DisplayName { get; set; } = null!;
}

