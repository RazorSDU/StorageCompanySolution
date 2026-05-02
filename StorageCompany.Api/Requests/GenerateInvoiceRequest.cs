using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Api.Requests;

public class GenerateInvoiceRequest
{
    [Required]
    public Guid RentalId { get; set; }

    [Required]
    public DateTime DueDateUtc { get; set; }
}
