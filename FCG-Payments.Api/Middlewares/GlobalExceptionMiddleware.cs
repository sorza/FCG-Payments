using Microsoft.AspNetCore.Mvc;

namespace FCG_Payments.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado");

                context.Response.ContentType = "application/problem+json";

                var statusCode = ex switch
                {
                    NotImplementedException => StatusCodes.Status501NotImplemented,
                    TimeoutException => StatusCodes.Status504GatewayTimeout,
                    InvalidOperationException => StatusCodes.Status502BadGateway,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;

                var problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = "Erro interno",
                    Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde."
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        }

    }
}
