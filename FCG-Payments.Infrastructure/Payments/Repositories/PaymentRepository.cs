using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using FCG_Payments.Infrastructure.Shared.Context;
using FCG_Payments.Infrastructure.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FCG_Payments.Infrastructure.Payments.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly PaymentDbContext _context;
        public PaymentRepository(PaymentDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Payment?> GetAsync(Func<Payment, bool> value, CancellationToken cancellationToken)
            => await _context.Payments.FirstOrDefaultAsync(p => value(p), cancellationToken);

        public async Task<IEnumerable<Payment>> GetAllAsync(Expression<Func<Payment, bool>> predicate, CancellationToken cancellationToken)
        => await _context.Payments.Where(predicate).ToListAsync(cancellationToken);
        

    }
}
