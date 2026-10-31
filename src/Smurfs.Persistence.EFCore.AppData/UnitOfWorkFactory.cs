using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

internal class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<DataContext> _contextFactory;

    public UnitOfWorkFactory(
        IDbContextFactory<DataContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public IUnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork(_contextFactory.CreateDbContext());
    }
}