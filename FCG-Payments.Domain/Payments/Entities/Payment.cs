using FCG_Payments.Domain.Payments.Enums;
using FCG_Payments.Domain.Payments.Exceptions.Payments;
using FCG_Payments.Domain.Shared;

namespace FCG_Payments.Domain.Payments.Entities
{
    public class Payment : Entity
    {
        #region Properties

        public Guid OrderId { get; private set; }
        public EPaymentType PaymentType { get; private set; }
        public EStatus Status { get; private set; }

        #endregion

        #region Constructors
        private Payment(Guid id) : base(id) { }

        private Payment(Guid id, Guid orderId, EPaymentType paymentType, EStatus status)
            : base(id)
        {
            OrderId = orderId;
            PaymentType = paymentType;
            Status = status;
        }

        #endregion

        #region Factory Methods
        public static Payment Create(Guid orderId, EPaymentType paymentType)
        {
            if (orderId == Guid.Empty)
                throw new OrderIdEmptyException(ErrorMessage.Payment.OrderIdIsEmpty);

            if (!Enum.IsDefined(typeof(EPaymentType), paymentType))
                throw new InvalidPaymentException(ErrorMessage.Payment.InvalidPaymentType);

            return new Payment(Guid.NewGuid(), orderId, paymentType, EStatus.Pending);
        }
        #endregion
               

        #region Methods
        public void UpdateStatus(EStatus newStatus)
        {
            if (!Enum.IsDefined(typeof(EStatus), newStatus))
                throw new InvalidStatusException(ErrorMessage.Payment.InvalidStatus);
            Status = newStatus;
            UpdateLastDateChanged();
        }        

        #endregion

    }
}
