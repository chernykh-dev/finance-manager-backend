using System.Security.Claims;
using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Services;

public interface IAuthService
{
    public string GetAccessToken(User user, out DateTime expiresAt);

    public string GetRefreshToken();

    public ClaimsPrincipal GetPrincipalFromToken(string accessToken);
}