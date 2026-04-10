using System.Diagnostics;
using EzraToDo.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EzraToDo.Api.Middleware;

/// <summary>
/// Global exception handler that converts various application exceptions into standardized RFC 7807 Problem Details responses.
/// Essential for production readiness and consistent API error reporting.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception has occurred.");

        var problemDetails = CreateProblemDetails(exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            FluentValidation.ValidationException validationException => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation failures have occurred.",
                Extensions =
                {
                    ["errors"] = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                }
            },
            EntityNotFoundException entityNotFoundException => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = entityNotFoundException.Message
            },
            _ => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            }
        };
    }
}
