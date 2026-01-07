using Azure.Messaging.ServiceBus;
using FCG.Shared.Contracts.Events.Publisher;
using FCG.Shared.Contracts.Events.Store;
using FCG.Shared.Contracts.Interfaces;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Infrastructure.Payments.Repositories;
using FCG_Payments.Infrastructure.Payments.Strategy;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FCG_Payments.Infrastructure.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
          

            var connectionString = configuration["ServiceBus:ConnectionString"];
            var queueName = configuration["ServiceBus:Topics:Payments"];

            services.AddSingleton(new ServiceBusClient(connectionString));

            services.AddScoped<IEventPublisher>(sp =>
            {
                var client = sp.GetRequiredService<ServiceBusClient>();
                return new ServiceBusEventPublisher(client, queueName!);
            });

            var mongoString = configuration["MongoSettings:ConnectionString"];
            var mongoDb = configuration["MongoSettings:Database"];
            var mongoCollection = configuration["MongoSettings:Collection"];

            services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoString));

            services.AddScoped<IEventStore>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return new MongoEventStore(client, mongoDb!, mongoCollection!);
            });

            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentResolver, PaymentFactory>();

            return services;
        }
    }
}
