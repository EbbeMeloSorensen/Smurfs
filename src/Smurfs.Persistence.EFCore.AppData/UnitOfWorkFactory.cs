using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

internal class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<DbContextAppData> _dbContextFactory;

    public UnitOfWorkFactory(IDbContextFactory<DbContextAppData> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public IUnitOfWork GenerateUnitOfWork()
    {
        return new UnitOfWork(_dbContextFactory.CreateDbContext());
    }
}