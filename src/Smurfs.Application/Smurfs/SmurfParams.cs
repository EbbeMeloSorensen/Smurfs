using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Carries paging parameters for a Smurf list query.
/// </summary>
public class SmurfParams : PagingParams
{
    /// <summary>
    /// Gets or sets a case-insensitive substring filter for the name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets selected infection states separated by pipes: true, false, or null.
    /// An omitted or blank value includes every state.
    /// </summary>
    public string? Infected { get; set; }
}
