using FCG.Shared.Contracts.Enums;

namespace FCG_Payments.Application.Payments.Requests
{
    public record PaymentRequest(List<Guid> LibraryItensId);
}
