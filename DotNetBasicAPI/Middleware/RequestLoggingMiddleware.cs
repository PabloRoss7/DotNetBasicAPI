using System.Diagnostics;

namespace DotNetBasicAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();

        // Dejamos correr el resto del pipeline (incluido el endpoint).
        await _next(context);

        // Recién acá conocemos el status final y el tiempo total.
        var elapsed = Stopwatch.GetElapsedTime(start);
        _logger.LogInformation(
            "HTTP {Method} {Path} respondió {StatusCode} en {Elapsed:0.0} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds);
    }
}
