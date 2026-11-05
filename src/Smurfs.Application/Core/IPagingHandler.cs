namespace Smurfs.Application.Core;

public interface IPagingHandler<T>
{
    Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize);

    PagedList<T> Create(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize);
}
