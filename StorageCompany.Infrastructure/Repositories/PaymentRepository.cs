using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class PaymentRepository : InMemoryRepository<Payment>, IPaymentRepository
{
    public PaymentRepository() : base(MockDatabase.Payments)
    {
    }

    public Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Payments
                .Where(payment => payment.CustomerId == customerId)
                .OrderByDescending(payment => payment.PaymentDateUtc)
                .ToList();

            return Task.FromResult<IReadOnlyList<Payment>>(result);
        }
    }

    public Task<IReadOnlyList<Payment>> GetByRentalIdAsync(Guid rentalId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Payments
                .Where(payment => payment.RentalId == rentalId)
                .OrderByDescending(payment => payment.PaymentDateUtc)
                .ToList();

            return Task.FromResult<IReadOnlyList<Payment>>(result);
        }
    }
}
