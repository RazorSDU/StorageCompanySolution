using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Api.Requests;

public class CreateDirectRentalRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid StorageUnitId { get; set; }

    [Required]
    public DateTime StartDateUtc { get; set; }
}
