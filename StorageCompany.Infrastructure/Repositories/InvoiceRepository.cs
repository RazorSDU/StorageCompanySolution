using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class InvoiceRepository : InMemoryRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository() : base(MockDatabase.Invoices)
    {
    }

    public Task<IReadOnlyList<Invoice>> GetByCustomerIdAsync(Guid customerId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Invoices
                .Where(invoice => invoice.CustomerId == customerId)
                .OrderByDescending(invoice => invoice.CreatedAtUtc)
                .ToList();

            return Task.FromResult<IReadOnlyList<Invoice>>(result);
        }
    }

    public Task<IReadOnlyList<Invoice>> GetByRentalIdAsync(Guid rentalId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Invoices
                .Where(invoice => invoice.RentalId == rentalId)
                .OrderByDescending(invoice => invoice.CreatedAtUtc)
                .ToList();

            return Task.FromResult<IReadOnlyList<Invoice>>(result);
        }
    }

    public Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        lock (MockDatabase.SyncRoot)
        {
            return Task.FromResult(MockDatabase.Invoices.FirstOrDefault(x => x.InvoiceNumber.Equals(invoiceNumber, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
