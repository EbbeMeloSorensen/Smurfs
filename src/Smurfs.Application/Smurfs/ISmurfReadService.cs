using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Reads pages of Smurf summaries without opening a write session.
/// </summary>
public interface ISmurfReadService
{
    /// <summary>
    /// Returns a page in the selected order with an identifier tie-breaker and a filtered total row count.
    /// Page number and page size must be positive.
    /// </summary>
    /// <param name="pageNumber">
    /// The one-based page number. A page beyond the result set is empty.
    /// </param>
    /// <param name="pageSize">
    /// The maximum number of items to return.
    /// </param>
    /// <param name="name">
    /// A case-insensitive literal name substring. Blank means all names.
    /// </param>
    /// <param name="infected">
    /// Infection states to include. Null or empty means all states.
    /// </param>
    /// <param name="newestFirst">
    /// Orders by creation timestamp descending when true, or by name ascending when false.
    /// </param>
    /// <param name="cancellationToken">
    /// Requests cancellation of retrieval.
    /// </param>
    Task<PagedList<SmurfDto>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken,
        string? name = null,
        IReadOnlyCollection<bool?>? infected = null,
        bool newestFirst = false);
}
