using Smurfs.Application.Core;
using Smurfs.Application.Smurfs;

namespace Smurfs.Persistence.File.Smurfs;

/// <summary>
/// Reads filtered, ordered pages from a single file snapshot without committing it.
/// </summary>
public sealed class SmurfReadService(IUnitOfWorkFactory factory) : ISmurfReadService
{
    /// <inheritdoc />
    public async Task<PagedList<SmurfDto>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken,
        string? name = null,
        IReadOnlyCollection<bool?>? infected = null,
        bool newestFirst = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        await using var session = factory.CreateUnitOfWork();
        var rows = await session.Smurfs.FindAsync([], cancellationToken);
        IEnumerable<Domain.VillageLife.Smurf> query = rows;
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(smurf => smurf.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (infected is { Count: > 0 })
        {
            query = query.Where(smurf => infected.Contains(smurf.Infected));
        }

        var filtered = query.ToArray();
        var offset = (pageNumber - 1L) * pageSize;
        if (offset >= filtered.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new PagedList<SmurfDto>([], filtered.Length, pageNumber, pageSize);
        }

        var ordered = newestFirst
            ? filtered.OrderByDescending(smurf => smurf.CreatedUtc)
            : filtered.OrderBy(smurf => smurf.Name, StringComparer.Ordinal);
        var items = ordered.ThenBy(smurf => smurf.Id).Skip((int) offset).Take(pageSize)
            .Select(smurf => new SmurfDto
            {
                Id = smurf.Id,
                Name = smurf.Name,
                CreatedUtc = smurf.CreatedUtc,
                Health = smurf.Health,
                Birthday = smurf.Birthday,
                Infected = smurf.Infected,
                Gender = smurf.Gender,
                PetId = smurf.PetId
            }).ToArray();
        cancellationToken.ThrowIfCancellationRequested();
        return new PagedList<SmurfDto>(items, filtered.Length, pageNumber, pageSize);
    }
}
