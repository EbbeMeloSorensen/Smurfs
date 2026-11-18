using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.AppData;

/// <summary>
/// Adapts the selected provider factory to the shared catalog context contract.
/// </summary>
internal sealed class AppDataContextFactory<TContext>(IDbContextFactory<TContext> factory)
    : IDbContextFactory<DataContext>
    where TContext : DataContext
{
    public DataContext CreateDbContext() => factory.CreateDbContext();

    public async Task<DataContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        await factory.CreateDbContextAsync(cancellationToken);
}
