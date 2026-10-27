namespace FinanceManagerBackend.API.HttpPipelines.Idempotency;

/// <summary>
/// Enables request body buffering for requests with Idempotency-Key,
/// so <see cref="IdempotencyFilter"/> can hash the body after model binding has read it.
/// </summary>
public class IdempotencyBufferingMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invoke middleware.
    /// </summary>
    /// <param name="httpContext"></param>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.ContainsKey(IdempotencyFilter.HeaderName))
        {
            httpContext.Request.EnableBuffering();
        }

        await next(httpContext);
    }
}
