using NotificationManager.Domain.Utilities.Exceptions;

namespace NotificationManager.Api.Middleware
{
    public class ExceptionHandlingMiddleware(
        RequestDelegate _next,
        ILogger<ExceptionHandlingMiddleware> _logger)
    {

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                context.Response.ContentType = "application/json";

                context.Response.StatusCode = ex switch
                {
                    TooManyRequestsLocallyException => StatusCodes.Status429TooManyRequests,
                    _ => StatusCodes.Status500InternalServerError
                };

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex switch
                    {
                        TooManyRequestsLocallyException =>
                            "Rate limit exceeded. Please try again later.",
                        _ => "An unexpected error occurred."
                    },
                    traceId = context.TraceIdentifier
                });
            }
        }
    }
}
