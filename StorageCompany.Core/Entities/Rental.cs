using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Entities;

public class Rental : EntityBase
{
    public Guid UserId { get; set; }
    public Guid StorageUnitId { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public decimal MonthlyPrice { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Active;
}
