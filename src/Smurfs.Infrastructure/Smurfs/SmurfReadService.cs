using Microsoft.EntityFrameworkCore;
using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;
using Smurfs.Persistence.EFCore.AppData;

namespace Smurfs.Infrastructure.Smurfs;

/// <summary>
/// Projects pages of Smurf summaries in the app-data database.
/// </summary>
/// <param name="context">
/// The app-data context supplied by the caller's scope.
/// </param>
/// <remarks>
/// Counts rows and reads the projected page in separate queries without tracking.
/// </remarks>
public sealed class SmurfReadService(DataContext context) : ISmurfReadService
{
    /// <inheritdoc />
    public async Task<PagedList<SmurfDto>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var smurfs = context.Smurfs.AsNoTracking();
        var count = await smurfs.CountAsync(cancellationToken);
        var offset = (pageNumber - 1L) * pageSize;

        if (offset >= count)
        {
            return new PagedList<SmurfDto>([], count, pageNumber, pageSize);
        }

        var items = await smurfs
            .OrderBy(smurf => smurf.Name)
            .ThenBy(smurf => smurf.Id)
            .Skip((int) offset)
            .Take(pageSize)
            .Select(smurf => new SmurfDto
            {
                Id = smurf.Id,
                Name = smurf.Name,
                Age = smurf.Age
            })
            .ToListAsync(cancellationToken);

        return new PagedList<SmurfDto>(items, count, pageNumber, pageSize);
    }
}
