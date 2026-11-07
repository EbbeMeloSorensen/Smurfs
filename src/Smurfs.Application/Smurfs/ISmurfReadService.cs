using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Reads pages of Smurf summaries without opening a write session.
/// </summary>
public interface ISmurfReadService
{
    /// <summary>
    /// Returns a page ordered by name and then identifier, with a total row count.
    /// Page number and page size must be positive.
    /// </summary>
    Task<PagedList<SmurfDto>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
