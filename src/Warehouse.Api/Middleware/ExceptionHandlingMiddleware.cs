using System.Net;
using System.Text.Json;

namespace Warehouse.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var detailedMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "An error occurred: {Message}", detailedMessage);
                
                await Handle(context, ex);
            }
        }

        private static async Task Handle(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorMessage = exception.InnerException?.Message ?? exception.Message;

            var errorDetail = new
            {
                Status = context.Response.StatusCode,
                Title = "An unexpected error has occured,",
                Error = new[] { errorMessage }, 
            };

            var errorJson = JsonSerializer.Serialize(errorDetail);
            await context.Response.WriteAsync(errorJson);
        }
    }
}