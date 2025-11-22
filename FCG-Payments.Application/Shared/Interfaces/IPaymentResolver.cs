using FCG.Shared.Contracts.Enums;

namespace FCG_Payments.Application.Shared.Interfaces
{
    internal interface IPaymentResolver
    {
         IPaymentStrategy Resolve(EPaymentType type);        
    }
}
