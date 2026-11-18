namespace Smurfs.API.DTOs;

/// <summary>
/// Contains the credentials submitted to the sign-in endpoint.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Gets or sets the email address used to identify the account.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the password submitted for authentication.
    /// </summary>
    public required string Password { get; set; }
}
