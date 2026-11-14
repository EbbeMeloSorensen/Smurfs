using Microsoft.AspNetCore.Identity;

namespace Smurfs.Persistence.EFCore.Identity;

/// <summary>
/// Provides sample accounts for local development.
/// </summary>
public static class Seed
{
    /// <summary>
    /// Creates any missing sample accounts through Identity.
    /// </summary>
    public static async Task SeedData(UserManager<AppUser> userManager)
    {
        AppUser[] users =
        [
            new() { DisplayName = "Bob", UserName = "bob", Email = "bob@test.com" },
            new() { DisplayName = "Jane", UserName = "jane", Email = "jane@test.com" },
            new() { DisplayName = "Tom", UserName = "tom", Email = "tom@test.com" }
        ];

        foreach (var user in users)
        {
            if (await userManager.FindByNameAsync(user.UserName!) is not null ||
                await userManager.FindByEmailAsync(user.Email!) is not null)
            {
                continue;
            }

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not seed user {user.UserName}. {errors}");
            }
        }
    }
}
