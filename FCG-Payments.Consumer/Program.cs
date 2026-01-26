using Azure.Messaging.ServiceBus;
using FCG.Shared.Contracts.Events.Store;
using FCG.Shared.Contracts.Interfaces;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Consumer.Consumers;
using FCG_Payments.Infrastructure.Payments.Repositories;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace FCG_Payments.Consumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService(options =>
                    {
                        options.ConnectionString = context.Configuration["ApplicationInsights:ConnectionString"];
                    });

                    services.AddDbContext<PaymentDbContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                    var connectionString = context.Configuration["ServiceBus:ConnectionString"];
                    services.AddSingleton(new ServiceBusClient(connectionString));

                    var mongoString = context.Configuration["MongoSettings:ConnectionString"];
                    var mongoDb = context.Configuration["MongoSettings:Database"];
                    var mongoCollection = context.Configuration["MongoSettings:Collection"];

                    services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoString));

                    services.AddScoped<IEventStore>(sp =>
                    {
                        var client = sp.GetRequiredService<IMongoClient>();
                        return new MongoEventStore(client, mongoDb!, mongoCollection!);
                    });

                    services.AddScoped<IPaymentRepository, PaymentRepository>();
                    services.AddHostedService<PaymentsTopicConsumer>(); 
                    services.AddHostedService<LibrariesTopicConsumer>();
                });

            await builder.RunConsoleAsync();
        }
    }
}