using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Smurfs.API.Services;
using Microsoft.IdentityModel.Tokens;
using Smurfs.Persistence.EFCore.Identity;

namespace Smurfs.API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment environment)
    {
        services.AddIdentityCore<AppUser>(opt => {
            opt.Password.RequireNonAlphanumeric = false;

        })
            .AddEntityFrameworkStores<DataContext>()
            .AddSignInManager<SignInManager<AppUser>>();

        var tokenKey = config["TokenKey"];
        if (tokenKey is null && environment.IsDevelopment())
        {
            tokenKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        if (string.IsNullOrWhiteSpace(tokenKey) || Encoding.UTF8.GetByteCount(tokenKey) < 64)
        {
            throw new InvalidOperationException("Configure TokenKey with at least 64 bytes.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha512],
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        services.AddSingleton(new SigningCredentials(key, SecurityAlgorithms.HmacSha512));
        services.AddScoped<TokenService>();

        return services;
    }
}
