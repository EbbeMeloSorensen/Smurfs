using System.ComponentModel.DataAnnotations;

namespace Smurfs.API.DTOs;

public class RegisterDto
{
    [Required]
    public required string DisplayName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [RegularExpression("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password must be complex")]
    // (?=.*\\d)     At least on number
    // (?=.*[a-z])   At least on lowercase letter
    // (?=.*[A-Z])   At least on uppercase letter
    // .{4,8}        From 4 to 8 characters
    public required string Password { get; set; }

    [Required]
    public required string Username { get; set; }
}
