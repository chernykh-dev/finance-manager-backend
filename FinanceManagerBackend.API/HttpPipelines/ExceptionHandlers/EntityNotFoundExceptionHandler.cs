using FinanceManagerBackend.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.HttpPipelines.ExceptionHandlers;

/// <summary>
/// Maps <see cref="EntityNotFoundException"/> to 404.
/// </summary>
public class EntityNotFoundExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<EntityNotFoundExceptionHandler> logger
) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not EntityNotFoundException)
        {
            return false;
        }

        logger.LogWarning(exception, "Entity not found");

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Entity not found",
                Status = StatusCodes.Status404NotFound
            }
        });
    }
}
