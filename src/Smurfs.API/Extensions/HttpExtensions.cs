using System.Text.Json;

namespace Smurfs.API.Extensions;

/// <summary>
/// Provides helpers for adding application metadata to HTTP responses.
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Writes the current page, page size, total item count, and total page count to the pagination header.
    /// </summary>
    public static void AddPaginationHeader(this HttpResponse response, int currentPage,
        int itemsPerPage, int totalItems, int totalPages)
    {
        var paginationHeader = new
        {
            currentPage,
            itemsPerPage,
            totalItems,
            totalPages
        };
        response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader));
        response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
    }
}
