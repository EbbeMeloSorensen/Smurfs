using Microsoft.EntityFrameworkCore;
using Smurfs.Application.Core;

namespace Smurfs.Infrastructure.Pagination;

/// <summary>
/// Pages EF-backed queries or in-memory sequences using their supplied order.
/// </summary>
public class PagingHandler<T> : IPagingHandler<T>
{
    /// <summary>
    /// Executes a count query and a separate page query through EF Core.
    /// </summary>
    public async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Counts the supplied sequence and enumerates the requested slice.
    /// </summary>
    public PagedList<T> Create(
        IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
