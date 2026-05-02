using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<IReadOnlyList<Invoice>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Invoice>> GetByRentalIdAsync(Guid rentalId);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
}
