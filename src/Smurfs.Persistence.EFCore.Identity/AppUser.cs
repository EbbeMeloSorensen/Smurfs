using Microsoft.AspNetCore.Identity;

namespace Smurfs.Persistence.EFCore.Identity;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = null!;
}

