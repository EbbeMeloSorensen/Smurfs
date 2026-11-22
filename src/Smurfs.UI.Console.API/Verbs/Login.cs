using CommandLine;

namespace Smurfs.UI.Console.API.Verbs;

/// <summary>
/// Authenticates once and saves a token for later command processes.
/// </summary>
[Verb("login", HelpText = "Sign in and save a token for this API address.")]
public sealed class Login
{
    /// <summary>
    /// Gets or sets the email; prompted interactively when omitted.
    /// </summary>
    [Option('e', "email", HelpText = "Account email.")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets whether to read a password line from standard input without prompting.
    /// </summary>
    [Option("password-stdin", HelpText = "Read one password line from stdin; requires --email.")]
    public bool PasswordStdin { get; set; }
}
