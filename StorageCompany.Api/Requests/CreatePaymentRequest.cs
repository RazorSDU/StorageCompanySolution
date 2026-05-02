using System.ComponentModel.DataAnnotations;
using StorageCompany.Core.Enums;

namespace StorageCompany.Api.Requests;

public class CreatePaymentRequest
{
    [Required]
    public Guid RentalId { get; set; }

    public Guid? InvoiceId { get; set; }

    [Range(1, double.MaxValue)]
    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Mock;
}
