using FCG_Payments.Application.Payments.Requests;
using FluentValidation;

namespace FCG_Payments.Application.Payments.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator()
        {
           
        }
    }
}
