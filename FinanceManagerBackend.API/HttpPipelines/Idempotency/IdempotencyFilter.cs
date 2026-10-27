using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FinanceManagerBackend.API.Infrastructure;
using FinanceManagerBackend.API.Infrastructure.EntityConfigurations;
using FinanceManagerBackend.API.Infrastructure.Idempotency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FinanceManagerBackend.API.HttpPipelines.Idempotency;

/// <summary>
/// Handles Idempotency-Key header on mutating requests (POST/PUT/PATCH/DELETE).
///
/// | Situation                                   | Response                                  |
/// |---------------------------------------------|-------------------------------------------|
/// | key is new                                  | execute, store result, return it          |
/// | key exists, same request hash               | return stored result, execute nothing     |
/// | key exists, different request hash         | 409                                       |
/// | key is held by a request still in progress  | 503 + Retry-After (must not be 4xx:       |
/// |                                             | the client treats 4xx as terminal)        |
/// | Redis is unavailable                        | handled by PostgreSQL alone (no fast path)|
///
/// Two layers:
/// - Redis (<see cref="IIdempotencyCache"/>): fast path. A completed response is replayed without touching
///   PostgreSQL; an in-flight request holds a short lock, so concurrent duplicates get 503 immediately.
///   Redis may lose data or be down at any time, nothing relies on it for correctness: without it requests
///   go straight to PostgreSQL.
/// - PostgreSQL (idempotency_keys): source of truth. The key row and the action's effects are written in one
///   DB transaction: either both are committed or neither is. Controllers that start their own transaction
///   get a savepoint inside this one (see <see cref="EntityRepository{TEntity}.BeginTransaction"/>).
///   5xx results and unhandled exceptions roll everything back, so the key can be retried.
/// </summary>
public class IdempotencyFilter(
    ServiceDbContext context,
    IIdempotencyCache cache,
    IOptions<JsonOptions> jsonOptions,
    ILogger<IdempotencyFilter> logger
) : IAsyncActionFilter
{
    /// <summary>
    /// Header name.
    /// </summary>
    public const string HeaderName = "Idempotency-Key";

    /// <summary>
    /// Response header set on replayed responses.
    /// </summary>
    public const string ReplayedHeaderName = "Idempotent-Replayed";

    /// <summary>
    /// How long a request waits for another in-flight request with the same key.
    /// </summary>
    private const string LockTimeout = "5s";

    /// <summary>
    /// Redis "in progress" lock lifetime: longer than any normal request. If a request outlives it,
    /// the PostgreSQL row lock (<see cref="LockTimeout"/>) still keeps duplicates out.
    /// </summary>
    private static readonly TimeSpan InProgressLockTimeToLive = TimeSpan.FromSeconds(30);

    private const int RetryAfterSeconds = 2;

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
    {
        var httpContext = actionContext.HttpContext;
        var request = httpContext.Request;
        var cancellationToken = httpContext.RequestAborted;

        if (!IsMutating(request.Method) || !request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            await next();
            return;
        }

        var key = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(key) || key.Length > IdempotencyRecordConfiguration.KeyMaxLength)
        {
            actionContext.Result = Problem(StatusCodes.Status400BadRequest,
                $"{HeaderName} must be a non-empty string up to {IdempotencyRecordConfiguration.KeyMaxLength} characters");
            return;
        }

        var userClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userClaim == null || !Guid.TryParse(userClaim.Value, out var userId))
        {
            // Anonymous endpoints (auth) are not covered.
            await next();
            return;
        }

        var requestHash = await ComputeRequestHashAsync(request, cancellationToken);

        string? lockToken;
        try
        {
            var cached = await cache.GetAsync(userId, key);
            if (cached != null)
            {
                actionContext.Result = Replay(httpContext, cached, requestHash);
                return;
            }

            lockToken = await cache.TryAcquireLockAsync(userId, key, InProgressLockTimeToLive);

            if (lockToken == null)
            {
                logger.LogInformation("Request with {Header}={Key} is still in progress", HeaderName, key);

                actionContext.Result = InProgress(httpContext);
                return;
            }
        }
        catch (IdempotencyCacheUnavailableException exception)
        {
            // No fast path: PostgreSQL alone still guarantees one effect per key
            // (its row lock makes a concurrent duplicate wait, then replay or 503).
            logger.LogWarning(exception, "Idempotency cache is unavailable, using PostgreSQL only");

            await ExecuteAsync(actionContext, next, userId, key, requestHash, useCache: false, cancellationToken);
            return;
        }

        try
        {
            await ExecuteAsync(actionContext, next, userId, key, requestHash, useCache: true, cancellationToken);
        }
        finally
        {
            await TryCacheAsync(() => cache.ReleaseLockAsync(userId, key, lockToken));
        }
    }

    private async Task ExecuteAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next,
        Guid userId, string key, string requestHash, bool useCache, CancellationToken cancellationToken)
    {
        var httpContext = actionContext.HttpContext;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.Database.ExecuteSqlRawAsync($"SET LOCAL lock_timeout = '{LockTimeout}'", cancellationToken);

        var createdAt = DateTimeOffset.UtcNow;

        int inserted;
        try
        {
            // Waits while another uncommitted transaction holds the same key.
            inserted = await context.Database.ExecuteSqlAsync(
                $"""
                 INSERT INTO idempotency_keys (user_id, key, request_hash, created_at)
                 VALUES ({userId}, {key}, {requestHash}, {createdAt})
                 ON CONFLICT (user_id, key) DO NOTHING
                 """,
                cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.LockNotAvailable)
        {
            logger.LogInformation("Request with {Header}={Key} is still in progress", HeaderName, key);

            actionContext.Result = InProgress(httpContext);
            return;
        }

        if (inserted == 0)
        {
            // Not in the cache (evicted, Redis restarted, or the cache write failed), but PostgreSQL has it.
            var record = await context.IdempotencyRecords
                .AsNoTracking()
                .SingleAsync(x => x.UserId == userId && x.Key == key, cancellationToken);

            if (record.RequestHash != requestHash)
            {
                actionContext.Result = DifferentRequest();
                return;
            }

            if (record.ResponseStatusCode == null)
            {
                actionContext.Result = InProgress(httpContext);
                return;
            }

            var stored = new IdempotencyResponse(record.RequestHash, record.ResponseStatusCode.Value,
                record.ResponseContentType, record.ResponseBody);

            if (useCache)
            {
                await CacheResponseAsync(userId, key, stored, record.CreatedAt);
            }

            actionContext.Result = Replay(httpContext, stored, requestHash);
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception != null && !executedContext.ExceptionHandled)
        {
            // Disposing the transaction rolls back both the effects and the key.
            return;
        }

        var (statusCode, contentType, body) = SerializeResult(executedContext.Result);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            return;
        }

        await context.IdempotencyRecords
            .Where(x => x.UserId == userId && x.Key == key)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.ResponseStatusCode, statusCode)
                    .SetProperty(x => x.ResponseContentType, contentType)
                    .SetProperty(x => x.ResponseBody, body),
                cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        // After commit only: the cache must never hold a response PostgreSQL doesn't have.
        if (useCache)
        {
            await CacheResponseAsync(userId, key,
                new IdempotencyResponse(requestHash, statusCode, contentType, body), createdAt);
        }
    }

    private async Task CacheResponseAsync(Guid userId, string key, IdempotencyResponse response,
        DateTimeOffset createdAt)
    {
        var timeToLive = createdAt + IdempotencyKeyCleanupJob.KeyTimeToLive - DateTimeOffset.UtcNow;

        if (timeToLive > TimeSpan.Zero)
        {
            await TryCacheAsync(() => cache.SetAsync(userId, key, response, timeToLive));
        }
    }

    /// <summary>
    /// Cache writes after the decision is made are best effort: PostgreSQL already holds the truth,
    /// and an unreleased lock expires by itself.
    /// </summary>
    private async Task TryCacheAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (IdempotencyCacheUnavailableException exception)
        {
            logger.LogWarning(exception, "Idempotency cache write failed");
        }
    }

    private static IActionResult Replay(HttpContext httpContext, IdempotencyResponse stored, string requestHash)
    {
        if (stored.RequestHash != requestHash)
        {
            return DifferentRequest();
        }

        httpContext.Response.Headers[ReplayedHeaderName] = "true";

        return new ContentResult
        {
            StatusCode = stored.StatusCode,
            ContentType = stored.ContentType,
            Content = stored.Body
        };
    }

    private static IActionResult DifferentRequest()
    {
        // Client bug: one key must always carry the same request.
        return Problem(StatusCodes.Status409Conflict, $"{HeaderName} was already used with a different request");
    }

    private static IActionResult InProgress(HttpContext httpContext)
    {
        httpContext.Response.Headers.RetryAfter = RetryAfterSeconds.ToString();

        return Problem(StatusCodes.Status503ServiceUnavailable,
            $"Request with this {HeaderName} is still in progress");
    }

    private (int StatusCode, string? ContentType, string? Body) SerializeResult(IActionResult? result)
    {
        switch (result)
        {
            case ObjectResult objectResult:
                var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
                var contentType = objectResult.Value is ProblemDetails
                    ? "application/problem+json"
                    : "application/json";
                var body = JsonSerializer.Serialize(objectResult.Value, objectResult.DeclaredType ?? objectResult.Value?.GetType() ?? typeof(object),
                    jsonOptions.Value.JsonSerializerOptions);

                return (statusCode, contentType, body);

            case IStatusCodeActionResult statusCodeResult:
                return (statusCodeResult.StatusCode ?? StatusCodes.Status200OK, null, null);

            default:
                return (StatusCodes.Status200OK, null, null);
        }
    }

    private static async Task<string> ComputeRequestHashAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        // Body is buffered by IdempotencyBufferingMiddleware, model binding has already read it.
        request.Body.Position = 0;

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes($"{request.Method} {request.Path}\n"));

        var buffer = new byte[8192];
        int read;
        while ((read = await request.Body.ReadAsync(buffer, cancellationToken)) > 0)
        {
            hash.AppendData(buffer, 0, read);
        }

        request.Body.Position = 0;

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static bool IsMutating(string method)
    {
        return HttpMethods.IsPost(method) || HttpMethods.IsPut(method) ||
               HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);
    }

    private static ObjectResult Problem(int statusCode, string title)
    {
        return new ObjectResult(new ProblemDetails { Status = statusCode, Title = title })
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}
