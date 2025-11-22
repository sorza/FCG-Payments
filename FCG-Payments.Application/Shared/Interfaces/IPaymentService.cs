using FCG.Shared.Contracts;
using FCG_Payments.Application.Payments.Responses;
using FCG_Payments.Application.Payments.Validators;
using FCG_Payments.Application.Shared.Results;

namespace FCG_Payments.Application.Shared.Interfaces
{
    public interface IPaymentService
    {
        public Task<Result<PaymentResponse>> CreateAsync(LibraryOrderEvent orderEvent, CancellationToken cancellationToken = default);
        public Task<Result<PaymentResponse>> PayAsync(Guid paymentId, CancellationToken cancellationToken = default);
    }
}
