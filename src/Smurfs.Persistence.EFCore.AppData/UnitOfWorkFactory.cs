using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Creates independent units of work through an EF Core context factory.
/// </summary>
public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<DataContext> _contextFactory;

    /// <summary>
    /// Stores the factory used to create a context per session.
    /// </summary>
    /// <param name="contextFactory">
    /// The non-null factory; ownership remains with its caller.
    /// </param>
    public UnitOfWorkFactory(
        IDbContextFactory<DataContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public IUnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork(_contextFactory.CreateDbContext());
    }
}
