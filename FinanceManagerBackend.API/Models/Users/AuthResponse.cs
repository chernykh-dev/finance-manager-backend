namespace FinanceManagerBackend.API.Models.Users;

public class AuthResponse
{
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
}