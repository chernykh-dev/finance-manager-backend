using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.HttpPipelines;

public static class ControllerBaseExtensions
{
    public static Guid GetUserIdFromClaims(this ControllerBase controller)
    {
        var userClaims = controller.User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userClaims == null || !Guid.TryParse(userClaims.Value, out var userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to access this resource");
        }

        return userId;
    }
}