using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Payment>> GetByRentalIdAsync(Guid rentalId);
}
