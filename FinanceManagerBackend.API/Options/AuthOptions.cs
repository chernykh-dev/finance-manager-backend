namespace FinanceManagerBackend.API.Options;

/// <summary>
/// Auth configuration options.
/// </summary>
public class AuthOptions
{
    /// <summary>
    /// Auth issuer.
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Auth audience.
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Auth secret key.
    /// </summary>
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// Auth expiration time (in minutes).
    /// </summary>
    public int ExpirationTimeInMinutes { get; set; }
}