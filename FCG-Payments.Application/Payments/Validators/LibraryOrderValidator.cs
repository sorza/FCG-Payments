using FCG.Shared.Contracts;
using FluentValidation;

namespace FCG_Payments.Application.Payments.Validators
{    
    public class LibraryOrderValidator : AbstractValidator<LibraryOrderEvent>
    {
        public LibraryOrderValidator()
        {
            RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("O código do pedido deve ser informado.");
            
            RuleFor(x => x.PaymentType)
                .IsInEnum().WithMessage("O tipo de pagamento informado é inválido.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("O status do pedido informado é inválido.");

            RuleFor(x => x.PricePaid)
                .GreaterThanOrEqualTo(0).When(x => x.PricePaid.HasValue).WithMessage("O valor pago não pode ser negativo.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("O código do usuário deve ser informado.");

            RuleFor(x => x.GameId)
                .NotEmpty().WithMessage("O código do jogo deve ser informado.");

        }
    }
}
