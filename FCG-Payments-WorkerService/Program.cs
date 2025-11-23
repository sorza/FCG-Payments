using Azure.Messaging.ServiceBus;
using FCG_Payments.Application.Payments.Services;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Infrastructure.Payments.Repositories;
using FCG_Payments.Infrastructure.Shared.Context;
using FCG_Payments_WorkerService.Consumers;
using Microsoft.EntityFrameworkCore;

namespace FCG_Payments_WorkerService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<PaymentDbContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                    var connectionString = context.Configuration["ServiceBus:ConnectionString"];
                    services.AddSingleton(new ServiceBusClient(connectionString));

                    services.AddScoped<IPaymentRepository, PaymentRepository>();

                    services.AddHostedService<LibraryEventsConsumer>();
                });

            await builder.RunConsoleAsync();
        }
    }
}