using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Api.Requests;

public class CreateRentalFromReservationRequest
{
    [Required]
    public Guid ReservationId { get; set; }
}
