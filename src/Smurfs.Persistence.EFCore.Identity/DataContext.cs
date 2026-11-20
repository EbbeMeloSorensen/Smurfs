using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Stores Identity users and related Identity records.
/// </summary>
public abstract class DataContext : IdentityDbContext<AppUser>
{
    /// <summary>
    /// Creates an Identity context using the supplied EF Core options.
    /// </summary>
    protected DataContext(
        DbContextOptions options) : base(options)
    {
    }
}
