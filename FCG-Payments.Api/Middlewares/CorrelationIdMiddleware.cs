namespace FCG_Payments.Api.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verifica se já existe um CorrelationId no header
            if (!context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Append("X-Correlation-Id", correlationId);
            }

            // Adiciona no Response também
            context.Response.Headers.Append("X-Correlation-Id", correlationId);

            // Disponibiliza no Items para uso interno
            context.Items["CorrelationId"] = correlationId;

            await _next(context);
        }
    }
}
