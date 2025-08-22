using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using FinanceManagerBackend.API.Configuration;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManagerBackend.API.Services;

public class AuthService(IOptions<AuthOptions> authOptions) : IAuthService
{
    public string GetAccessToken(User user, out DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString())
        };

        expiresAt = DateTime.UtcNow.AddMinutes(authOptions.Value.ExpirationTimeInMinutes);

        var jwt = new JwtSecurityToken(
            issuer: authOptions.Value.Issuer,
            audience: authOptions.Value.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(AuthHelper.GetSymmetricSecurityKey(authOptions.Value.SecretKey),
                SecurityAlgorithms.HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public string GetRefreshToken()
    {
        var randomNumber = new byte[32];

        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Value.Issuer,

            ValidateAudience = true,
            ValidAudience = authOptions.Value.Audience,

            ValidateLifetime = true,

            IssuerSigningKey = AuthHelper.GetSymmetricSecurityKey(authOptions.Value.SecretKey)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals
                (SecurityAlgorithms.HmacSha256,StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}