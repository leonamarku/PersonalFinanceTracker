using System.Net;
using System.Text.Json;
using PersonalFinanceTracker.Core.Common;

namespace PersonalFinanceTracker.API.Middleware;

/// <summary>
/// Global error handler: catches unhandled exceptions and returns a consistent
/// JSON error response. Domain ServiceExceptions map to their embedded status codes.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ServiceException ex)
        {
            _logger.LogWarning(ex, "Domain exception: {Message}", ex.Message);
            await WriteErrorAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext ctx, int statusCode, string message)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode  = statusCode;
        var body = JsonSerializer.Serialize(new { error = message, statusCode });
        await ctx.Response.WriteAsync(body);
    }
}
