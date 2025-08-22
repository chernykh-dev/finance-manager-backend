namespace FinanceManagerBackend.API.Options;

public class AuthOptions
{
    public string Issuer { get; set; }

    public string Audience { get; set; }

    public string SecretKey { get; set; }

    public int ExpirationTimeInMinutes { get; set; }
}