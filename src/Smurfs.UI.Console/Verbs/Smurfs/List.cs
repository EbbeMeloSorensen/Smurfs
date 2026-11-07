using CommandLine;

namespace Smurfs.UI.Console.Verbs.Smurfs;

[Verb("listsmurfs", HelpText = "List a page of Smurfs.")]
public sealed class List
{
    /// <summary>
    /// Gets or sets the one-based page to display.
    /// </summary>
    [Option('p', "page", Default = 1, HelpText = "Page number.")]
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the requested page size, capped at 50 by the application.
    /// </summary>
    [Option('s', "page-size", Default = 25, HelpText = "Page size (maximum 50).")]
    public int PageSize { get; set; }
}
