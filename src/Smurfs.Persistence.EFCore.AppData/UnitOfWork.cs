using Smurfs.Persistence.Repositories;
using Smurfs.Persistence.EFCore.AppData.Repositories;

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

    public void Complete()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}