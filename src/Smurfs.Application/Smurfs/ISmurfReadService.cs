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
    /// <param name="pageNumber">
    /// The one-based page number. A page beyond the result set is empty.
    /// </param>
    /// <param name="pageSize">
    /// The maximum number of items to return.
    /// </param>
    /// <param name="cancellationToken">
    /// Requests cancellation of retrieval.
    /// </param>
    Task<PagedList<SmurfDto>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
