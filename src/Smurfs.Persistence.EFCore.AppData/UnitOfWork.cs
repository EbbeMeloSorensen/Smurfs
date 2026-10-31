using Smurfs.Persistence.EFCore.AppData.Repositories;
using Smurfs.Persistence.Repositories;

namespace Smurfs.Persistence.EFCore.AppData;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public ISmurfRepository Smurfs { get; }

    public UnitOfWork(
        DataContext context)
    {
        _context = context;

        Smurfs = new SmurfRepository(_context);
    }

    public async Task CompleteAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            _context.SaveChanges();
        }, cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}