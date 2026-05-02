using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Entities;

public class Invoice : EntityBase
{
    public Guid RentalId { get; set; }
    public Guid CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDateUtc { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
}
