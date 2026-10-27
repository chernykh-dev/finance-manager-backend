using FinanceManagerBackend.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.HttpPipelines.ExceptionHandlers;

/// <summary>
/// Maps <see cref="EntityExistsException"/> to 409.
/// </summary>
public class EntityExistsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<EntityExistsExceptionHandler> logger
) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not EntityExistsException)
        {
            return false;
        }

        logger.LogWarning(exception, "Entity already exists");

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Entity already exists",
                Status = StatusCodes.Status409Conflict
            }
        });
    }
}
