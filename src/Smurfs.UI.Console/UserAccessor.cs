using Smurfs.Application.Interfaces;

namespace Smurfs.UI.Console;

/// <summary>
/// Reserves a console implementation of the current-user contract for future integration.
/// </summary>
public class UserAccessor : IUserAccessor
{
    /// <summary>
    /// Throws because the console does not currently resolve a username.
    /// </summary>
    public string GetUsername()
    {
        throw new NotImplementedException();
    }
}
