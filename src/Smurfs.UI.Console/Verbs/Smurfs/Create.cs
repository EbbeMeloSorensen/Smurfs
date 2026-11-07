using CommandLine;

namespace Smurfs.UI.Console.Verbs.Smurfs;

/// <summary>
/// Represents the console request to create a Smurf.
/// </summary>
[Verb("createsmurf", HelpText = "Create a new Smurf.")]
public sealed class Create
{
    /// <summary>
    /// Gets or sets the name to send to the application command.
    /// </summary>
    [Option('n', "name", Required = true, HelpText = "Name")]
    public required string Name { get; set; }
}
