using FCG_Payments.Domain.Payments.Enums;

namespace FCG_Payments.Application.Shared.Interfaces
{
    public interface IPaymentGateway
    {
        public Task<bool> Pay(Guid OrderId, EPaymentType type);
    }
}
