using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.HttpPipelines.ExceptionHandlers;

public class ValidationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ValidationExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        // Expected client error: no stack trace, not an error-level entry.
        logger.LogInformation("Validation failed for {Method} {Path}: {Errors}", httpContext.Request.Method,
            httpContext.Request.Path, string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage)));

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Detail = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest
            }
        };

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        context.ProblemDetails.Extensions.Add("errors", errors);

        // todo make this work.
        // await problemDetailsService.TryWriteAsync(context);

        await httpContext.Response.WriteAsJsonAsync(context.ProblemDetails, cancellationToken);

        return true;
    }
}