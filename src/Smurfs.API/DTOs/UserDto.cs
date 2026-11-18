namespace Smurfs.API.DTOs;

/// <summary>
/// Contains the account information returned to an authenticated client.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the name shown to the user.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the JWT used to authenticate subsequent API requests.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the account username.
    /// </summary>
    public required string Username { get; set; }
}
