using FCG_Payments.Infrastructure.Shared;
using FCG_Payments.Application.Shared;
using System.Reflection;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.EntityFrameworkCore;
using FCG_Payments.Api.Middlewares;

namespace FCG_Payments.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
           // builder.WebHost.UseUrls("http://0.0.0.0:80");

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

            builder.Services.AddHttpClient("LibrariesApi", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:LibrariesApi"]!);
            });

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<CorrelationIdMiddleware>();

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

            app.Run();
        }
    }
}
