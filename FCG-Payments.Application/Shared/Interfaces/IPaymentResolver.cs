using FCG.Shared.Contracts.Enums;

namespace FCG_Payments.Application.Shared.Interfaces
{
    public interface IPaymentResolver
    {
         IPaymentStrategy Resolve(EPaymentType type);        
    }
}
