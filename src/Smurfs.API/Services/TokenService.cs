using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Smurfs.Persistence.EFCore.Identity;

namespace Smurfs.API.Services;

/// <summary>
/// Issues signed JWTs for Smurfs accounts.
/// </summary>
public class TokenService
{
    private readonly SigningCredentials _signingCredentials;

    /// <summary>
    /// Creates the token issuer with the configured signing credentials.
    /// </summary>
    public TokenService(SigningCredentials signingCredentials)
    {
        _signingCredentials = signingCredentials;
    }

    /// <summary>
    /// Issues a signed token containing the account identity claims.
    /// </summary>
    public string CreateToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName ?? throw new InvalidOperationException("The account has no username.")),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? throw new InvalidOperationException("The account has no email address."))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
