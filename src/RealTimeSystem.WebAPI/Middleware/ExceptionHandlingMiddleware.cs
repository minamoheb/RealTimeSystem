using System.Text.Json;
using RealTimeSystem.Application.Common.Exceptions;
using RealTimeSystem.Domain.Exceptions;

namespace RealTimeSystem.WebAPI.Middleware
{
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (status, title, errors) = Map(exception);
            if (status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogWarning("Request failed with {StatusCode}: {Message}", status, exception.Message);
            }

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            var payload = new
            {
                title,
                status,
                detail = status >= StatusCodes.Status500InternalServerError && !_environment.IsDevelopment()
                    ? "An unexpected error occurred."
                    : exception.Message,
                errors,
                traceId = context.TraceIdentifier
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
    }

    private static (int Status, string Title, IDictionary<string, string[]>? Errors) Map(Exception exception) => exception switch
    {
        ValidationAppException validation => (StatusCodes.Status400BadRequest, "Validation failed", validation.Errors),
        UnauthorizedAppException => (StatusCodes.Status401Unauthorized, "Unauthorized", null),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", null),
        NotFoundException => (StatusCodes.Status404NotFound, "Not found", null),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict", null),
        DomainException => (StatusCodes.Status422UnprocessableEntity, "Domain rule violated", null),
        _ => (StatusCodes.Status500InternalServerError, "Internal server error", null)
    };
}
}

