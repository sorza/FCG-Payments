using FCG.Shared.Contracts;
using FCG.Shared.Contracts.Enums;
using FCG_Payments.Application.Payments.Responses;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Application.Shared.Results;
using FCG_Payments.Domain.Payments.Entities;

namespace FCG_Payments.Application.Payments.Services
{
    public class PaymentService(IRepository<Payment> repository,
                                IPaymentResolver resolver,
                                IEventPublisher publisher) : IPaymentService
    {      

        public async Task<Result<PaymentResponse>> PayAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            if (paymentId == Guid.Empty)
                return Result.Failure<PaymentResponse>(new Error("400", "Código de pagamento inválido"));

            var payment = await repository.GetByIdAsync(paymentId, cancellationToken);

            if(payment is null)
                return Result.Failure<PaymentResponse>(new Error("404","Pagamento não encontrado"));

            var paymentStrategy = resolver.Resolve(payment.PaymentType);
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
