using CommandLine;

namespace Smurfs.UI.Console.API.Verbs;

/// <summary>
/// Removes the locally saved token for the configured API address.
/// </summary>
[Verb("logout", HelpText = "Remove the saved token for this API address.")]
public sealed class Logout
{
}
