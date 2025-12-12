using System.Net;
using System.Text.Json;

namespace BookLendingService.Api.Middlewares
{
    /// <summary>
    /// Middleware that handles unhandled exceptions globally, logs the error, and returns a standardized error response.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var correlationId = context.TraceIdentifier;

                _logger.LogError(
                    ex,
                    "Unhandled exception. CorrelationId: {CorrelationId}, Path: {Path}",
                    correlationId,
                    context.Request.Path);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var problem = new
                {
                    status = context.Response.StatusCode,
                    title = "An unexpected error occurred.",
                    correlationId,
                    traceId = correlationId,
                    detail = _env.IsDevelopment() ? ex.ToString() : null
                };

                var json = JsonSerializer.Serialize(problem);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
