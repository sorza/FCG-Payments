
using Azure.Messaging.ServiceBus;
using FCG.Shared.Contracts;
using FCG.Shared.Contracts.Enums;
using FCG_Payments.Application.Payments.Services;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using FCG_Payments.Infrastructure.Payments.Repositories;
using System.Text.Json;
using System.Threading;

namespace FCG_Payments_WorkerService.Consumers
{
    public class LibraryEventsConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;

        public LibraryEventsConsumer(ServiceBusClient client, IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            var queueName = config["ServiceBus:Queues:LibrariesEvents"];
            _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += async args =>
            {
                var body = args.Message.Body.ToString();
                var subject = args.Message.Subject;

                if (subject == "OrderCreated")
                {
                    var evt = JsonSerializer.Deserialize<LibraryOrderEvent>(body);

                    if (evt is not null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                        var payment = Payment.Create(evt.ItemId, evt.PaymentType);

                        await repo.AddAsync(payment, stoppingToken);

                        Console.WriteLine($"O pagamento do pedido {evt.ItemId} foi gerado com sucesso!");
                    }
                }

                await args.CompleteMessageAsync(args.Message);
            };

            _processor.ProcessErrorAsync += args =>
            {
                Console.WriteLine($"Erro no consumer: {args.Exception.Message}");
                return Task.CompletedTask;
            };

            await _processor.StartProcessingAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
