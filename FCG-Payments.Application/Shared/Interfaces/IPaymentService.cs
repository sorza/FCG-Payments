using FCG.Shared.Contracts.Enums;
using FCG.Shared.Contracts.Results;
using FCG_Payments.Application.Payments.Requests;
using FCG_Payments.Application.Payments.Responses;

namespace FCG_Payments.Application.Shared.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<PaymentResponse>> CreatePaymentAsync(PaymentRequest request, string correlationId, CancellationToken cancellationToken = default);
        Task<Result<PaymentResponse>> PayAsync(Guid paymentId, EPaymentType paymentType, string correlationId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<PaymentResponse>?>> GetApprovedPaymentsAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<PaymentResponse>?>> GetFailedPaymentsAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<PaymentResponse>?>> GetAllPaymentsAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<PaymentResponse>?>> GetPendingPaymentsAsync(CancellationToken cancellationToken);
        Task<Result<PaymentResponse>?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken);       
    }
}
