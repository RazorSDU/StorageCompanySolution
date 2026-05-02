using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IInvoiceService
{
    Task<Invoice> GenerateForRentalAsync(Guid rentalId, DateTime dueDateUtc);
    Task<Invoice> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Invoice>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Invoice>> GetByRentalIdAsync(Guid rentalId);
    Task<Invoice> MarkAsPaidAsync(Guid invoiceId);
}
