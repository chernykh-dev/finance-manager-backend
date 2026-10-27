namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <summary>
/// Stored result of a request with Idempotency-Key, as kept in the cache.
/// </summary>
/// <param name="RequestHash">SHA-256 (hex) of method, path and body of the original request.</param>
/// <param name="StatusCode">Response status code.</param>
/// <param name="ContentType">Response content type.</param>
/// <param name="Body">Response body.</param>
public record IdempotencyResponse(string RequestHash, int StatusCode, string? ContentType, string? Body);
