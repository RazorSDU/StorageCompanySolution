using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Entities;

public class Reservation : EntityBase
{
    public Guid CustomerId { get; set; }
    public Guid StorageUnitId { get; set; }
    public DateTime ReservationDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime MoveInDateUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
}
