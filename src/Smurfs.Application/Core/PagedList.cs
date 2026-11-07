namespace Smurfs.Application.Core;

/// <summary>
/// Contains the items and metadata for one page of a larger result set.
/// </summary>
public class PagedList<T> : List<T>
{
    /// <summary>
    /// Copies the supplied page items and computes the total page count from the total item count and positive page size.
    /// </summary>
    public PagedList(IEnumerable<T> items,
        int count, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        TotalPages = (int) Math.Ceiling(count / (double) pageSize);
        PageSize = pageSize;
        TotalCount = count;
        AddRange(items);
    }

    /// <summary>
    /// Gets or sets the requested one-based page number.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the number of pages needed to cover the full result set.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets the requested maximum number of items on a page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the number of matching items across all pages.
    /// </summary>
    public int TotalCount { get; set; }
}
