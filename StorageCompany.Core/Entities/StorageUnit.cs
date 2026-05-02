using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Entities;

public class StorageUnit : EntityBase
{
    public Guid FacilityId { get; set; }
    public Guid UnitTypeId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal MonthlyPrice { get; set; }
    public StorageUnitStatus Status { get; set; } = StorageUnitStatus.Available;
    public bool IsClimateControlled { get; set; }
    public bool IsDriveUp { get; set; }
}
