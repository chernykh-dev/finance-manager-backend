namespace FinanceManagerBackend.API.Models.Users;

/// <summary>
/// Auth response model.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Auth access token.
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Auth refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}