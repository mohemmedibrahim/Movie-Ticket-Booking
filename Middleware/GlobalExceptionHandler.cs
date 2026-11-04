using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Exceptions;

namespace MovieTicketBooking.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            NotFoundException e => (StatusCodes.Status404NotFound, "Not Found", e.Message),
            BadRequestException e => (StatusCodes.Status400BadRequest, "Bad Request", e.Message),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflict", e.Message),
            UnauthorizedException e => (StatusCodes.Status401Unauthorized, "Unauthorized", e.Message),
            ForbiddenException e => (StatusCodes.Status403Forbidden, "Forbidden", e.Message),
            // e.g. unique-index violation from two simultaneous requests (V1 has no full concurrency control)
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflict",
                "The request conflicts with the current state of the data. Please refresh and try again."),
            _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
        };

        if (status >= 500) logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else logger.LogWarning("{Title} on {Method} {Path}: {Detail}", title, httpContext.Request.Method, httpContext.Request.Path, detail);

        var problem = new ProblemDetails
        {
            Status = status, Title = title, Detail = detail, Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
