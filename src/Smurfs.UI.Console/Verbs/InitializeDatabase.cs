using CommandLine;

namespace Smurfs.UI.Console.Verbs;

/// <summary>
/// Requests database migration and sample data initialization.
/// </summary>
[Verb("initdb", HelpText = "Apply database migrations and seed sample data.")]
public sealed class InitializeDatabase
{
}
