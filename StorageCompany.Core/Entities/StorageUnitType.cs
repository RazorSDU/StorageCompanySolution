namespace StorageCompany.Core.Entities;

public class StorageUnitType : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal SizeInSquareMeters { get; set; }
    public string Description { get; set; } = string.Empty;
    public string RecommendedFor { get; set; } = string.Empty;
}
