using CommandLine;

namespace Smurfs.UI.Console.Verbs.Smurfs;

[Verb("createsmurf", HelpText = "Create a new Smurf.")]
public sealed class Create
{
    [Option('n', "name", Required = true, HelpText = "Name")]
    public required string Name { get; set; }
}
