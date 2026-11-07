using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Carries paging parameters for a Smurf list query.
/// </summary>
public class SmurfParams : PagingParams
{
    /// <summary>
    /// Gets or sets a potential name filter. The current list handler does not apply it.
    /// </summary>
    public string? Name { get; set; }
}
