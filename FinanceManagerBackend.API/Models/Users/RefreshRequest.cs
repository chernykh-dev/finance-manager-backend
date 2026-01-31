namespace FinanceManagerBackend.API.Models.Users;

/// <summary>
/// Refresh auth token request model.
/// </summary>
public class RefreshRequest
{
    /// <summary>
    /// Auth refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}