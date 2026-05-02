using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class PaymentService : IPaymentService
{
    private readonly IRentalRepository _rentals;
    private readonly IPaymentRepository _payments;
    private readonly IInvoiceRepository _invoices;

    public PaymentService(IRentalRepository rentals, IPaymentRepository payments, IInvoiceRepository invoices)
    {
        _rentals = rentals;
        _payments = payments;
        _invoices = invoices;
    }


    public async Task<Payment> GetByIdAsync(Guid id)
    {
        var payment = await _payments.GetByIdAsync(id);
        return payment ?? throw new NotFoundException($"Payment '{id}' was not found.");
    }

    public async Task<Payment> CreateMockPaymentAsync(Guid rentalId, decimal amount, PaymentMethod paymentMethod, Guid? invoiceId = null)
    {
        Guard.AgainstEmpty(rentalId, nameof(rentalId));
        Guard.AgainstNonPositive(amount, nameof(amount));

        var rental = await _rentals.GetByIdAsync(rentalId)
            ?? throw new NotFoundException($"Rental '{rentalId}' was not found.");

        if (rental.Status != RentalStatus.Active)
            throw new BusinessRuleException("Payments can only be made for active rentals.");

        Invoice? invoice = null;
        if (invoiceId.HasValue)
        {
            invoice = await _invoices.GetByIdAsync(invoiceId.Value)
                ?? throw new NotFoundException($"Invoice '{invoiceId}' was not found.");

            if (invoice.RentalId != rental.Id)
                throw new BusinessRuleException("The invoice does not belong to the selected rental.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            CustomerId = rental.CustomerId,
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Paid,
            PaymentDateUtc = DateTime.UtcNow,
            TransactionReference = $"MOCK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            CreatedAtUtc = DateTime.UtcNow
        };

        if (invoice is not null)
        {
            invoice.Status = InvoiceStatus.Paid;
            await _invoices.UpdateAsync(invoice);
        }

        await _payments.AddAsync(payment);
        return payment;
    }

    public Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId)
    {
        return _payments.GetByCustomerIdAsync(customerId);
    }

    public Task<IReadOnlyList<Payment>> GetByRentalIdAsync(Guid rentalId)
    {
        return _payments.GetByRentalIdAsync(rentalId);
    }
}
