namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <summary>
/// The idempotency cache (Redis) is unavailable.
/// </summary>
/// <param name="innerException"></param>
public class IdempotencyCacheUnavailableException(Exception innerException)
    : Exception("Idempotency cache is unavailable", innerException);
