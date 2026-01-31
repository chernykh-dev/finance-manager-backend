using System.Security.Claims;
using FinanceManagerBackend.API.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManagerBackend.API.Services;

/// <summary>
/// Auth service.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Returns access token for user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="expiresAt">Token expires at.</param>
    /// <returns></returns>
    string GetAccessToken(User user, out DateTime expiresAt);

    /// <summary>
    /// Returns refresh token for current user.
    /// </summary>
    /// <returns></returns>
    string GetRefreshToken();

    /// <summary>
    /// Returns claims principal from access token.
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    /// <exception cref="SecurityTokenException"></exception>
    ClaimsPrincipal GetPrincipalFromToken(string accessToken);
}