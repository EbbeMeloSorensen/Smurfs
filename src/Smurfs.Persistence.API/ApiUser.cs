namespace Smurfs.Persistence.API;

/// <summary>
/// Contains account display information without exposing its bearer token.
/// </summary>
public sealed class ApiUser
{
    /// <summary>
    /// Gets the authenticated account's display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the authenticated account's username.
    /// </summary>
    public required string Username { get; init; }
}
