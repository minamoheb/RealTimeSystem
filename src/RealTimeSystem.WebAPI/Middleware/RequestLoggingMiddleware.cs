using System.Diagnostics;

namespace RealTimeSystem.WebAPI.Middleware
{
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var startDate = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();
        var endDate = DateTime.UtcNow;
        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms. StartDate {StartDate:o} EndDate {EndDate:o}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            startDate,
            endDate);
    }
}
}

