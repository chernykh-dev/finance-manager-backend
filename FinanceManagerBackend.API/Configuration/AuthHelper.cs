using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManagerBackend.API.Configuration;

/// <summary>
/// Helper methods for auth.
/// </summary>
public static class AuthHelper
{
    /// <summary>
    /// Get symmetric security key for signing.
    /// </summary>
    /// <param name="key"></param>
    public static SymmetricSecurityKey GetSymmetricSecurityKey(string key) =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

    /// <summary>
    /// Encrypt user password.
    /// </summary>
    public static string EncryptPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verify input user password with stored encrypted password.
    /// </summary>
    public static bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}