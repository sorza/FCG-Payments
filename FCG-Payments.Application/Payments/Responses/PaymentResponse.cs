using FCG.Shared.Contracts.Enums;

namespace FCG_Payments.Application.Payments.Responses
{
    public sealed record PaymentResponse(Guid Id, List<Guid> ItensId, EPaymentType PaymentType, EPaymentStatus Status, decimal price);
}
