using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Interfaces.Services;

public interface IPaymentService
{
    Task<Payment> GetByIdAsync(Guid id);
    Task<Payment> CreateMockPaymentAsync(Guid rentalId, decimal amount, PaymentMethod paymentMethod, Guid? invoiceId = null);
    Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Payment>> GetByRentalIdAsync(Guid rentalId);
}
