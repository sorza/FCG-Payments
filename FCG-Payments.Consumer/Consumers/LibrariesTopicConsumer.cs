using Azure.Messaging.ServiceBus;
using FCG.Shared.Contracts.Events.Domain.Payments;
using FCG.Shared.Contracts.Interfaces;
using FCG_Payments.Application.Shared.Interfaces;
using System.Text.Json;

namespace FCG_Payments.Consumer.Consumers
{
    public class LibrariesTopicConsumer : IHostedService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LibrariesTopicConsumer> _logger;

        public LibrariesTopicConsumer(ServiceBusClient client, IConfiguration cfg, IServiceScopeFactory scopeFactory, ILogger<LibrariesTopicConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;           

            var topicName = cfg["ServiceBus:Topics:Libraries"];
            var subscriptionName = cfg["ServiceBus:Subscriptions:Libraries"];

            _processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 4,
                PrefetchCount = 20
            });

            _processor.ProcessMessageAsync += OnMessageAsync;
            _processor.ProcessErrorAsync += OnErrorAsync;
        }

        private async Task OnMessageAsync(ProcessMessageEventArgs args)
        {
            var subject = args.Message.Subject;
            var body = args.Message.Body.ToString();

            _logger.LogInformation("Mensagem recebida: Subject={Subject}, CorrelationId={CorrelationId}", subject, args.Message.CorrelationId);

            switch (subject)
            {               
                case "PaymentCancelledEvent": await HandlePaymentCanceledEvent(body, args.Message.CorrelationId);
                    break;
                default:
                    _logger.LogWarning("Evento desconhecido: {Subject}", subject);
                    break;
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task HandlePaymentCanceledEvent(string body, string correlationId)
        {
            var evt = JsonSerializer.Deserialize<PaymentDeletedEvent>(body);

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
            var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

            var events = await eventStore.GetEventsAsync(evt!.AggregateId);
            var currentVersion = events.Count;

            var payment = await repo.GetByIdAsync(Guid.Parse(evt!.AggregateId));

            if (payment is not null)
            {
                await eventStore.AppendAsync(evt.AggregateId, evt, currentVersion + 1, correlationId);
                await repo.DeleteAsync(payment.Id);
                _logger.LogInformation($"Pagamento {payment.Id} cancelado e removido do sistema.");
            }

        }

        private Task OnErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Erro no consumer: {EntityPath}", args.EntityPath);
            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consumer iniciado para {Topic}/{Subscription}", _processor.EntityPath, "payments-libraries-sub");
            await _processor.StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consumer parado");
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }
}
