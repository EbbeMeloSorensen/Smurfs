using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Smurfs.Persistence.EFCore.Identity;

public class DataContext : IdentityDbContext<AppUser>
{
    public DataContext(
        DbContextOptions<DataContext> options) : base(options)
    {
    }
}
