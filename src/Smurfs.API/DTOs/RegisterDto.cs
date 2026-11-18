using System.ComponentModel.DataAnnotations;

namespace Smurfs.API.DTOs;

/// <summary>
/// Contains the account information and credentials submitted during registration.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Gets or sets the name displayed in the client.
    /// </summary>
    [Required]
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the email address associated with the new account.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the password for the new account.
    /// </summary>
    [Required]
    [RegularExpression("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password must be complex")]
    // (?=.*\\d)     At least on number
    // (?=.*[a-z])   At least on lowercase letter
    // (?=.*[A-Z])   At least on uppercase letter
    // .{4,8}        From 4 to 8 characters
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the unique username for the new account.
    /// </summary>
    [Required]
    public required string Username { get; set; }
}
