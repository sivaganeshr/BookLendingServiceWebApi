using System.Diagnostics;

namespace BookLendingService.Api.Middlewares
{
    /// <summary>
    /// Middleware that ensures each HTTP request has a correlation ID for end-to-end request tracing.
    /// </summary>
    /// <remarks>The <see cref="CorrelationIdMiddleware"/> checks for a correlation ID in the
    /// <c>X-Correlation-Id</c> request header. If none is present, it generates a new correlation ID. The correlation
    /// ID is set on <see cref="HttpContext.TraceIdentifier"/> and added to the response headers, and is included in the
    /// logging scope for the duration of the request. This enables consistent tracking of requests across distributed
    /// systems and improves observability in logs and diagnostics.</remarks>
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes an HTTP request by ensuring a correlation ID is present, setting it on the request and response,
        /// and logging request details within a correlation scope.
        /// </summary>
        /// <remarks>If the incoming request does not include a correlation ID in the
        /// <c>X-Correlation-Id</c> header, a new correlation ID is generated. The correlation ID is set on <see
        /// cref="HttpContext.TraceIdentifier"/> and added to the response headers. Logging for the request is performed
        /// within a scope that includes the correlation ID, enabling end-to-end tracing of requests across distributed
        /// systems.</remarks>
        /// <param name="context">The <see cref="HttpContext"/> for the current HTTP request.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            // Get existing correlation id from header(X-Correlation-Id) OR generate new one
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) ||
                string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Set it on TraceIdentifier and on response header
            context.TraceIdentifier = correlationId!;
            context.Response.Headers[CorrelationIdHeaderName] = correlationId!;

            // Optional: set Activity.Current for distributed tracing
            Activity.Current ??= new Activity("Request");
            Activity.Current.SetIdFormat(ActivityIdFormat.W3C);
            Activity.Current.TraceStateString = correlationId;

            // Log request start/end with correlation id in the scope
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId!
            }))
            {
                _logger.LogInformation("Handling HTTP {Method} {Path}", context.Request.Method, context.Request.Path);

                await _next(context);

                _logger.LogInformation("Finished handling HTTP {Method} {Path} with status {StatusCode}",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode);
            }
        }
    }
}
