namespace Smurfs.Application.Core;

/// <summary>
/// Defines generic paging for caller-supplied sequences. Callers provide positive paging values and any required ordering.
/// </summary>
public interface IPagingHandler<T>
{
    /// <summary>
    /// Counts and retrieves a page from a queryable source.
    /// </summary>
    Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Counts and selects a page from an enumerable source.
    /// </summary>
    PagedList<T> Create(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize);
}
