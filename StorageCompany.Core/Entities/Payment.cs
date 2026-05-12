using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Entities;

public class Payment : EntityBase
{
    public Guid RentalId { get; set; }
    public Guid UserId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDateUtc { get; set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string TransactionReference { get; set; } = string.Empty;
}
