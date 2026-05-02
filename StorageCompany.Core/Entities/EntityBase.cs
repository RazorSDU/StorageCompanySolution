namespace StorageCompany.Core.Entities;

public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
