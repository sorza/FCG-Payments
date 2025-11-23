using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;

namespace FCG_Payments.Infrastructure.Payments.Strategy
{
    public class PaymentStrategy : IPaymentStrategy
    {
        public Task<bool> Pay(Payment payment)
        {
            throw new NotImplementedException();
        }
    }
}
