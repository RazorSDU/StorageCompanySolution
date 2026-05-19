using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoices;
    private readonly IRentalRepository _rentals;

    public InvoiceService(IInvoiceRepository invoices, IRentalRepository rentals)
    {
        _invoices = invoices;
        _rentals = rentals;
    }

    public async Task<Invoice> GenerateForRentalAsync(Guid rentalId, DateTime dueDateUtc)
    {
        var rental = await _rentals.GetByIdAsync(rentalId)
            ?? throw new NotFoundException($"Rental '{rentalId}' was not found.");

        if (rental.Status != RentalStatus.Active)
            throw new BusinessRuleException("Invoices can only be generated for active rentals.");

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            UserId = rental.UserId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Amount = rental.MonthlyPrice,
            DueDateUtc = DateTime.SpecifyKind(dueDateUtc, DateTimeKind.Utc),
            Status = InvoiceStatus.Unpaid,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _invoices.AddAsync(invoice);
        return invoice;
    }

    public async Task<Invoice> GetByIdAsync(Guid id)
    {
        var invoice = await _invoices.GetByIdAsync(id);
        return invoice ?? throw new NotFoundException($"Invoice '{id}' was not found.");
    }

    public Task<IReadOnlyList<Invoice>> GetByCustomerIdAsync(Guid customerId)
    {
        return _invoices.GetByCustomerIdAsync(customerId);
    }

    public Task<IReadOnlyList<Invoice>> GetByRentalIdAsync(Guid rentalId)
    {
        return _invoices.GetByRentalIdAsync(rentalId);
    }

    public async Task<Invoice> MarkAsPaidAsync(Guid invoiceId)
    {
        var invoice = await GetByIdAsync(invoiceId);
        invoice.Status = InvoiceStatus.Paid;
        await _invoices.UpdateAsync(invoice);
        return invoice;
    }
}
