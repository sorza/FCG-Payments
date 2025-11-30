using FCG_Payments.Infrastructure.Shared;
using FCG_Payments.Application.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace FCG_Payments.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://0.0.0.0:80");

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });

            var app = builder.Build();

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var ex = exceptionHandlerPathFeature?.Error;

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
                });
            });

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                var retries = 5;
                while (retries > 0)
                {
                    try
                    {
                        db.Database.Migrate();
                        break;
                    }
                    catch
                    {
                        retries--;
                        Thread.Sleep(2000); 
                    }
                }
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/health", () =>
            {
                return Results.Ok(new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow
                });
            });

            app.Run();
        }
    }
}
