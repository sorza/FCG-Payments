using FCG.Shared.Contracts;
using FCG.Shared.Contracts.Enums;
using FCG_Payments.Application.Payments.Responses;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Application.Shared.Results;
using FCG_Payments.Domain.Payments.Entities;
using FluentValidation;

namespace FCG_Payments.Application.Payments.Services
{
    public class PaymentService(IRepository<Payment> repository,
                                IPaymentStrategy paymentStrategy,
                                IEventPublisher publisher,
                                IValidator<LibraryOrderEvent> orderValidator) : IPaymentService
    {
        public async Task<Result<PaymentResponse>> CreateAsync(LibraryOrderEvent orderEvent, CancellationToken cancellationToken = default)
        {
            var result = await orderValidator.ValidateAsync(orderEvent, cancellationToken);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => new Error("400", e.ErrorMessage)).ToList();
                return Result.Failure<PaymentResponse>(new Error("400", string.Join("; ", errors.Select(e => e.Message))));
            }

            var paymentType = Enum.TryParse(orderEvent.PaymentType.ToString(), out EPaymentType ePaymentType);

            if(!paymentType)
                return Result.Failure<PaymentResponse>(new Error("400","Forma de pagamento inválida"));

            var payment = Payment.Create(orderEvent.ItemId, ePaymentType);            

            await repository.AddAsync(payment, cancellationToken);

            return Result.Success(new PaymentResponse(payment.Id, payment.OrderId, payment.PaymentType, payment.Status));

        }

        public async Task<Result<PaymentResponse>> PayAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await repository.GetByIdAsync(paymentId, cancellationToken);

            if(payment is null)
                return Result.Failure<PaymentResponse>(new Error("404","Pagamento não encontrado"));

            var success = await paymentStrategy.Pay(payment);
                 
            if(!success)
            {
                var evt_fail = new PaymentProcessedEvent(
                   payment.Id,
                   payment.OrderId,
                   payment.PaymentType,
                   EPaymentStatus.Failed);

                await repository.UpdateAsync(payment, cancellationToken);

                await publisher.PublishAsync(evt_fail, "PaymentFailedEvent");

                return Result.Failure<PaymentResponse>(new Error("402", "Pagamento recusado"));
            }
           
            var evtSucesso = new PaymentProcessedEvent(
                payment.Id,
                payment.OrderId,
                payment.PaymentType,
                EPaymentStatus.Approved);

            payment.UpdateStatus(EPaymentStatus.Approved);            

            await repository.UpdateAsync(payment, cancellationToken);

            await publisher.PublishAsync(evtSucesso, "PaymentAprovedEvent");

            return Result.Success(new PaymentResponse(payment.Id, payment.OrderId, payment.PaymentType, payment.Status));
            
        }
    }
}
