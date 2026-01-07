using FCG_Payments.Application.Payments.Requests;
using FluentValidation;

namespace FCG_Payments.Application.Payments.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.LibraryItensId)
                .NotEmpty().WithMessage("A lista de itens da biblioteca não pode estar vazia.")
                .Must(list => list.All(id => id != Guid.Empty)).WithMessage("A lista de itens da biblioteca contém IDs inválidos.")
                .Must(list => list.Distinct().Count() == list.Count).WithMessage("A lista de itens da biblioteca contém IDs duplicados.")
                .Must(list => list.All(id => Guid.TryParse(id.ToString(), out _))).WithMessage("A lista de itens da biblioteca contém IDs inválidos.");
           
        }
    }
}
