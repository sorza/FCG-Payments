using Azure.Messaging.ServiceBus;
using FCG.Shared.Contracts.Events.Domain.Payments;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using System.Text.Json;

namespace FCG_Payments.Consumer.Consumers
{
    public class PaymentsTopicConsumer : IHostedService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentsTopicConsumer> _logger;

        public PaymentsTopicConsumer(ServiceBusClient client, IConfiguration cfg, IServiceScopeFactory scopeFactory, ILogger<PaymentsTopicConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var topicName = cfg["ServiceBus:Topics:Payments"];
            var subscriptionName = cfg["ServiceBus:Subscriptions:Payments"];

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
                case "PaymentCreated": await HandlePaymentCreatedEvent(body);
                    break;
                case "PaymentProcessed": await HandlePaymentProcessedEvent(body);
                    break;
                default:
                    _logger.LogWarning("Evento desconhecido: {Subject}", subject);
                    break;
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task HandlePaymentProcessedEvent(string body)
        {
            var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(body);

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

            var payment = await repo.GetByIdAsync(Guid.Parse(evt!.AggregateId));

            if(payment is not null)
            {
                payment.UpdateStatus(evt.PaymentStatus);
                payment.UpdatePaymentType(evt.PaymentType);

                await repo.UpdateAsync(payment);
                _logger.LogInformation("Pagamento atualizado: {PaymentId} para status {Status}", payment.Id, evt.PaymentStatus);
            }
            else
            {
                _logger.LogWarning("Pagamento não encontrado: {PaymentId}", evt.AggregateId);
            }

        }

        private async Task HandlePaymentCreatedEvent(string body)
        {
            var evt = JsonSerializer.Deserialize<PaymentCreatedEvent>(body);

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

            var item = Payment.Create(evt!.Price, Guid.Parse(evt!.AggregateId));
            
            var payment = await repo.GetByIdAsync(item.Id);

            if(payment is null)
            {
                await repo.AddAsync(item);
                _logger.LogInformation("Pagamento criado: {PaymentId}", item.Id);
            }
            else
            {
                _logger.LogInformation("Pagamento já existe: {PaymentId}", item.Id);
            }

        }

        private Task OnErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Erro no consumer: {EntityPath}", args.EntityPath);
            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consumer iniciado para {Topic}/{Subscription}", _processor.EntityPath, "payments-api-sub");
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
