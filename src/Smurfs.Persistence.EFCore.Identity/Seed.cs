using Microsoft.AspNetCore.Identity;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Provides sample accounts for a separate Identity host.
/// </summary>
public class Seed
{
    /// <summary>
    /// Attempts to create sample accounts if no users exist and writes creation errors to the console.
    /// </summary>
    public static async Task SeedData(
        DataContext context,
        UserManager<AppUser> userManager)
    {
        if (!userManager.Users.Any())
        {
            var users = new List<AppUser>
            {
                new AppUser
                {
                    DisplayName = "Bob",
                    UserName = "bob",
                    Email = "bob@test.com"
                },
                new AppUser
                {
                    DisplayName = "Jane",
                    UserName = "jane",
                    Email = "jane@test.com"
                },
                new AppUser
                {
                    DisplayName = "Tom",
                    UserName = "tom",
                    Email = "tom@test.com"
                },
            };

            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, "Super-long-very-secure-secret-key-that-is-at-least-64-bytes-in-length!!!!");

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
