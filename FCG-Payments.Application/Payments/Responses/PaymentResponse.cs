using FCG.Shared.Contracts.Enums;

namespace FCG_Payments.Application.Payments.Responses
{
    public sealed record PaymentResponse(Guid Id, Guid OrderId, EPaymentType PaymentType, EPaymentStatus Status);
}
