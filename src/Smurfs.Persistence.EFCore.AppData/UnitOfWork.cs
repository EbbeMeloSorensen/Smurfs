using Smurfs.Persistence.Repositories;
using Smurfs.Persistence.EFCore.AppData.Repositories;

namespace Smurfs.Persistence.EFCore.AppData;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContextAppData _context;

    public ISmurfRepository Smurfs { get; }

    public UnitOfWork(
        DbContextAppData context)
    {
        _context = context;

        Smurfs = new SmurfRepository(_context);
    }

    public void Complete()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}