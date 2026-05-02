namespace StorageCompany.Core.Entities;

public class AccessCode : EntityBase
{
    public Guid RentalId { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAtUtc { get; set; }
}
