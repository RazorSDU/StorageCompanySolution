using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Api.Requests;

public class CreateSupportRequestRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    public Guid? RentalId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}
