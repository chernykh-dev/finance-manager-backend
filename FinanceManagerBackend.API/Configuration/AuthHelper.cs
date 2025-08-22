using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManagerBackend.API.Configuration;

public class AuthHelper
{
    public static SymmetricSecurityKey GetSymmetricSecurityKey(string key) =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

    public static string EncryptPassword(string password)
    {
        var data = Encoding.UTF8.GetBytes(password);

        data = SHA256.HashData(data);

        return Encoding.UTF8.GetString(data);
    }
}