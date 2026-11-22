using CommandLine;

namespace Smurfs.UI.Console.Verbs;

/// <summary>
/// Requests storage initialization and sample data seeding for the selected provider.
/// </summary>
[Verb("initdb", HelpText = "Initialize the selected storage and seed sample data.")]
public sealed class InitializeDatabase
{
}
