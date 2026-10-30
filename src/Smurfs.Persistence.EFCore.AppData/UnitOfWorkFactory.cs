using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

internal class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<DataContext> _dbContextFactory;

    public UnitOfWorkFactory(IDbContextFactory<DataContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public IUnitOfWork GenerateUnitOfWork()
    {
        return new UnitOfWork(_dbContextFactory.CreateDbContext());
    }
}