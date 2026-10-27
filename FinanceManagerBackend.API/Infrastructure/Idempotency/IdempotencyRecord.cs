namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <summary>
/// Stored result of a mutating request sent with Idempotency-Key header.
/// </summary>
public class IdempotencyRecord
{
    /// <summary>
    /// Owner of the key: keys are unique per user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Idempotency-Key header value.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// SHA-256 (hex) of method, path and body of the original request.
    /// </summary>
    public string RequestHash { get; set; } = null!;

    /// <summary>
    /// Stored response status code (null while the request is being processed).
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// Stored response content type.
    /// </summary>
    public string? ResponseContentType { get; set; }

    /// <summary>
    /// Stored response body.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Created at, used for expiration.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
