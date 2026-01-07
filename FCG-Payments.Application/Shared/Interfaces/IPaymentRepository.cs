using FCG_Payments.Domain.Payments.Entities;
using System.Linq.Expressions;

namespace FCG_Payments.Application.Shared.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetAsync(Func<Payment, bool> value, CancellationToken cancellationToken);
        Task<IEnumerable<Payment>> GetAllAsync(Expression<Func<Payment, bool>> predicate, CancellationToken cancellationToken);
    }
}
