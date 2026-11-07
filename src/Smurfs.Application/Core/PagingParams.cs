namespace Smurfs.Application.Core;

/// <summary>
/// Carries one-based page selection and a capped page size.
/// </summary>
public abstract class PagingParams
{
    private const int MaxPageSize = 50;

    /// <summary>
    /// Gets or sets the requested page number, which defaults to one.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    private int _pageSize = 25;

    /// <summary>
    /// Gets or sets the page size, which defaults to 25. Values above 50 are capped.
    /// The caller or handler must reject nonpositive values.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}
