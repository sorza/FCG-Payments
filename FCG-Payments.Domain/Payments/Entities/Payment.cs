using FCG.Shared.Contracts.ClassDefinition;
using FCG.Shared.Contracts.Enums;
using FCG_Payments.Domain.Payments.Exceptions.Payments;

namespace FCG_Payments.Domain.Payments.Entities
{
    public class Payment : Entity
    {
        #region Properties
               
        public EPaymentType PaymentType { get; private set; }
        public EPaymentStatus Status { get; private set; }
        public decimal Price { get; private set; }

        #endregion

        #region Constructors
        private Payment(Guid id) : base(id) { }

        private Payment(Guid id, EPaymentStatus status, decimal price)
            : base(id)
        {
            PaymentType = EPaymentType.Pix;
            Status = status;
            Price = price;
        }

        #endregion

        #region Factory Methods
        public static Payment Create(decimal price, Guid Id = default)
        {        
            if (price <= 0)
                throw new ArgumentException("O valor do pagamento não pode ser negativo");

            if(Id == default) return new Payment(Guid.NewGuid(), EPaymentStatus.Pending, price);

            return new Payment(Id, EPaymentStatus.Pending, price);
        }
        
        #endregion

        #region Methods
        public void UpdateStatus(EPaymentStatus newStatus)
        {
            if (!Enum.IsDefined(typeof(EPaymentStatus), newStatus))
                throw new InvalidStatusException(ErrorMessage.Payment.InvalidStatus);
            Status = newStatus;
            UpdateLastDateChanged();
        }

        public void UpdatePaymentType(EPaymentType paymentType)
        {
            if (!Enum.IsDefined(typeof(EPaymentType), paymentType))
                throw new InvalidPaymentException(ErrorMessage.Payment.InvalidPaymentType);
            PaymentType = paymentType;
            UpdateLastDateChanged();
        }

        #endregion

    }
}
