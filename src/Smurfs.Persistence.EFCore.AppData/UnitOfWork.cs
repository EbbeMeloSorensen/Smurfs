using Smurfs.Persistence.Repositories;
using Smurfs.Persistence.EFCore.AppData.Repositories;

namespace Smurfs.Persistence.EFCore.AppData;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public ISmurfRepository Smurfs { get; }

    public UnitOfWork(
        DataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        Smurfs = new SmurfRepository(_context);
    }

    public async Task CompleteAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}