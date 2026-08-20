namespace Warehouse.Api.Middleware
{
    public class CorrelationIdMiddleware 
    {
        private readonly RequestDelegate _next;
        private const string Header = "X-Correlation-Id";
        public CorrelationIdMiddleware (RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context) 
        {
            string correlationId = GetOrCreateCorrelationId(context);
            context.Items["CorrelationId"] = correlationId;
            context.TraceIdentifier = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[Header] = correlationId;
                return Task.CompletedTask;
            });
            await _next(context);
        }
        private string GetOrCreateCorrelationId (HttpContext context)
        {
            context.Request.Headers.TryGetValue(CorrelationIdMiddleware.Header, out  var existingId);
            if(!string.IsNullOrWhiteSpace(existingId)) return existingId.ToString();

            return Guid.NewGuid().ToString();
        }

    }
}