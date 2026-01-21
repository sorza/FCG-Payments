using FCG.Shared.Contracts.Enums;
using FCG.Shared.Contracts.Events.Domain.Payments;
using FCG.Shared.Contracts.Interfaces;
using FCG.Shared.Contracts.Results;
using FCG_Payments.Application.Payments.Requests;
using FCG_Payments.Application.Payments.Responses;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using FluentValidation;
using System.Net.Http.Json;

namespace FCG_Payments.Application.Payments.Services
{
    public class PaymentService(IPaymentRepository repository,
                                IValidator<PaymentRequest> validator,
                                IPaymentResolver resolver,
                                IEventPublisher publisher,
                                IEventStore eventStore,
                                IHttpClientFactory httpClient) : IPaymentService
    {
        public async Task<Result<PaymentResponse>> CreatePaymentAsync(PaymentRequest request, string correlationId, CancellationToken cancellationToken = default)
        {
            var validation = validator.Validate(request);
            if (!validation.IsValid)
                return Result.Failure<PaymentResponse>(new Error("400", string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))));

            var librariesClient = httpClient.CreateClient("LibrariesApi");

            var itens = new List<LibraryResponse>();

            foreach (var libraryItemId in request.LibraryItensId)
            {
                var libraryItem = await librariesClient.GetAsync($"api/{libraryItemId}", cancellationToken);
                if(!libraryItem.IsSuccessStatusCode)
                    return Result.Failure<PaymentResponse>(new Error("404", $"Item de biblioteca {libraryItemId} não encontrado."));  
                
                itens.Add(libraryItem.Content.ReadFromJsonAsync<LibraryResponse>(cancellationToken: cancellationToken).Result!);
            }

            foreach (var item in itens)            
                if (item.Status == EOrderStatus.Owned)
                    return Result.Failure<PaymentResponse>(new Error("400", $"O item de biblioteca {item.ItemId} não está pendente de pagamento."));
            

            var valorTotal = itens.Sum(i => i.PricePaid ?? 0);

            var payment = Payment.Create(valorTotal);

            var evt = new PaymentCreatedEvent(payment.Id.ToString(), request.LibraryItensId, EPaymentType.Pix, payment.Status, valorTotal);

            await eventStore.AppendAsync(evt.AggregateId, evt, 0, correlationId);

            await publisher.PublishAsync(evt, "PaymentCreated", correlationId);

            return Result.Success(new PaymentResponse(payment.Id, request.LibraryItensId, payment.PaymentType, payment.Status, valorTotal));
        }

        public async Task<Result<PaymentResponse>> PayAsync(Guid paymentId, EPaymentType paymentType, string correlationId,CancellationToken cancellationToken = default)
        {            
            var payment = await repository.GetByIdAsync(paymentId, cancellationToken);
            if(payment is null)
                return Result.Failure<PaymentResponse>(new Error("404", "Pagamento não encontrado."));
           
            if (payment.Status == EPaymentStatus.Approved)
                return Result.Failure<PaymentResponse>(new Error("400", "Este pagamento já foi efetuado."));
            
            var librariesClient = httpClient.CreateClient("LibrariesApi");
           
            var itens = await librariesClient.GetAsync($"api/payments/{paymentId}", cancellationToken);

            if(!itens.IsSuccessStatusCode)
                return Result.Failure<PaymentResponse>(new Error("404", "Itens da biblioteca não encontrados para este pagamento."));

           
            var libraryItems = await itens.Content.ReadFromJsonAsync<List<LibraryResponse>>(cancellationToken: cancellationToken);
            var totalAmount = libraryItems!.Sum(i => i.PricePaid ?? 0);

            if(payment.Price != totalAmount)
                return Result.Failure<PaymentResponse>(new Error("400", "O valor do pagamento não corresponde ao valor total dos itens."));

           
            var paymentStrategy = resolver.Resolve(paymentType);
            var success = await paymentStrategy.Pay(payment);

            var events = await eventStore.GetEventsAsync(payment.Id.ToString());
            var currentVersion = events.Count;

            var evt = new PaymentProcessedEvent(payment.Id.ToString(), paymentType, EPaymentStatus.Failed);
           
            if (success) evt = new PaymentProcessedEvent(payment.Id.ToString(), paymentType, EPaymentStatus.Approved);

            await eventStore.AppendAsync(payment.Id.ToString(), evt, currentVersion, correlationId);

            await publisher.PublishAsync(evt, "PaymentProcessed", correlationId);

            if (!success) return Result.Failure<PaymentResponse>(new Error("402", "Pagamento recusado"));

            return Result.Success(new PaymentResponse(payment.Id, libraryItems!.Select(l => l.ItemId).ToList(), payment.PaymentType, payment.Status, totalAmount));
                        
        }   
              
        public async Task<Result<IEnumerable<PaymentResponse>?>> GetFailedPaymentsAsync(CancellationToken cancellationToken)
        {
            var payments = await repository.GetAllAsync(pay => pay.Status == EPaymentStatus.Failed, cancellationToken);
            var result = payments?.Select(p => new PaymentResponse(p.Id, [], p.PaymentType, p.Status, p.Price));

            return Result.Success(result);
        }

        public async Task<Result<IEnumerable<PaymentResponse>?>> GetPendingPaymentsAsync(CancellationToken cancellationToken)
        {
            var payments = await repository.GetAllAsync(pay => pay.Status == EPaymentStatus.Pending, cancellationToken);
            var result = payments?.Select(p => new PaymentResponse(p.Id, [], p.PaymentType, p.Status, p.Price));

            return Result.Success(result);
        }

        public async Task<Result<IEnumerable<PaymentResponse>?>> GetAllPaymentsAsync(CancellationToken cancellationToken)
        {
            var payments = await repository.GetAllAsync(cancellationToken);
            var result = payments?.Select(p => new PaymentResponse(p.Id, [], p.PaymentType, p.Status, p.Price));

            return Result.Success(result);
        }

        public async Task<Result<PaymentResponse>?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken)
        {
            var p = await repository.GetByIdAsync(paymentId, cancellationToken);

            if (p is null)
                return Result.Failure<PaymentResponse>(new Error("404", "Pagamento não encontrado."));

            return Result.Success(new PaymentResponse(p.Id, [], p.PaymentType, p.Status, p.Price));
        }

        public async Task<Result<IEnumerable<PaymentResponse>?>> GetApprovedPaymentsAsync(CancellationToken cancellationToken)
        {
            var payments = await repository.GetAllAsync(pay => pay.Status == EPaymentStatus.Approved, cancellationToken);
            var result = payments?.Select(p => new PaymentResponse(p.Id, [], p.PaymentType, p.Status, p.Price));

            return Result.Success(result);
        }
    }
    public sealed record LibraryResponse(Guid ItemId, Guid UserId, Guid GameId, EOrderStatus Status, decimal? PricePaid);
}
