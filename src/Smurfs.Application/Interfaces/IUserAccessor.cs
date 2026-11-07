namespace Smurfs.Application.Interfaces;

/// <summary>
/// Provides the current username to application use cases.
/// </summary>
public interface IUserAccessor
{
    /// <summary>
    /// Returns the current username supplied by the host.
    /// </summary>
    string GetUsername();
}
