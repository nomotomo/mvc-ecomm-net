using Serilog.Context;

namespace ApiGateway.Middleware;

public class CorrelationIdMiddleware
{   
    private const string CorrelationIdHeaderName = "x-correlation-id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate if not presetn
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers.Add(CorrelationIdHeaderName, correlationId);
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(CorrelationIdHeaderName, correlationId);
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Correlation ID: {CorrelationId}", correlationId);
            await _next(context);
        }
        
    }
}