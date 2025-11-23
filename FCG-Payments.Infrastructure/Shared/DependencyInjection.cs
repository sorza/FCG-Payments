using Azure.Messaging.ServiceBus;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using FCG_Payments.Infrastructure.Payments.Events;
using FCG_Payments.Infrastructure.Payments.Repositories;
using FCG_Payments.Infrastructure.Payments.Strategy;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG_Payments.Infrastructure.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
          

            var connectionString = configuration["ServiceBus:ConnectionString"];
            var queueName = configuration["ServiceBus:Queues:PaymentsEvents"];

            services.AddSingleton(new ServiceBusClient(connectionString));

            services.AddScoped<IEventPublisher>(sp =>
            {
                var client = sp.GetRequiredService<ServiceBusClient>();
                return new ServiceBusEventPublisher(client, queueName!);
            });

            services.AddScoped<IRepository<Payment>, PaymentRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentResolver, PaymentFactory>();
            services.AddScoped<IPaymentStrategy, PaymentStrategy>();

            return services;
        }
    }
}
