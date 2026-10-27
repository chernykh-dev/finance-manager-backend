using System.Text.Json;
using StackExchange.Redis;

namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <inheritdoc />
public class RedisIdempotencyCache(IConnectionMultiplexer redis) : IIdempotencyCache
{
    /// <inheritdoc />
    public async Task<IdempotencyResponse?> GetAsync(Guid userId, string key)
    {
        var value = await Execute(db => db.StringGetAsync(ResponseKey(userId, key)));

        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<IdempotencyResponse>(value.ToString());
    }

    /// <inheritdoc />
    public async Task SetAsync(Guid userId, string key, IdempotencyResponse response, TimeSpan timeToLive)
    {
        await Execute(db => db.StringSetAsync(ResponseKey(userId, key), JsonSerializer.Serialize(response),
            timeToLive));
    }

    /// <inheritdoc />
    public async Task<string?> TryAcquireLockAsync(Guid userId, string key, TimeSpan timeToLive)
    {
        var token = Guid.NewGuid().ToString("N");

        // SET key token NX PX ttl
        var acquired = await Execute(db => db.LockTakeAsync(LockKey(userId, key), token, timeToLive));

        return acquired ? token : null;
    }

    /// <inheritdoc />
    public async Task ReleaseLockAsync(Guid userId, string key, string token)
    {
        // Deletes only if the value is still our token: an expired lock re-taken by another request stays.
        await Execute(db => db.LockReleaseAsync(LockKey(userId, key), token));
    }

    private static RedisKey ResponseKey(Guid userId, string key) => $"idempotency:{userId:N}:{key}";

    private static RedisKey LockKey(Guid userId, string key) => $"idempotency-lock:{userId:N}:{key}";

    private async Task<T> Execute<T>(Func<IDatabase, Task<T>> operation)
    {
        // The multiplexer keeps reconnecting in the background; while it's down, fail at once
        // instead of waiting for a timeout on every request.
        if (!redis.IsConnected)
        {
            throw new IdempotencyCacheUnavailableException(new RedisConnectionException(
                ConnectionFailureType.UnableToConnect, "Redis is not connected"));
        }

        try
        {
            return await operation(redis.GetDatabase());
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            throw new IdempotencyCacheUnavailableException(exception);
        }
    }
}
