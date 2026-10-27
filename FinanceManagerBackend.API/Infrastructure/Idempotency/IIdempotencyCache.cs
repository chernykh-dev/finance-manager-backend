namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <summary>
/// Fast layer for Idempotency-Key handling: completed responses and "request in progress" locks.
/// PostgreSQL (idempotency_keys) stays the source of truth; the cache may lose data at any time.
/// </summary>
public interface IIdempotencyCache
{
    /// <summary>
    /// Returns cached response for the key, or null.
    /// </summary>
    /// <exception cref="IdempotencyCacheUnavailableException"></exception>
    Task<IdempotencyResponse?> GetAsync(Guid userId, string key);

    /// <summary>
    /// Caches response for the key.
    /// </summary>
    /// <exception cref="IdempotencyCacheUnavailableException"></exception>
    Task SetAsync(Guid userId, string key, IdempotencyResponse response, TimeSpan timeToLive);

    /// <summary>
    /// Marks the key as "request in progress". Returns lock token, or null if the key is already locked.
    /// </summary>
    /// <exception cref="IdempotencyCacheUnavailableException"></exception>
    Task<string?> TryAcquireLockAsync(Guid userId, string key, TimeSpan timeToLive);

    /// <summary>
    /// Releases the lock if it is still held with this token.
    /// </summary>
    /// <exception cref="IdempotencyCacheUnavailableException"></exception>
    Task ReleaseLockAsync(Guid userId, string key, string token);
}
