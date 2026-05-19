using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Entities;

public class SupportRequest : EntityBase
{
    public Guid UserId { get; set; }
    public Guid? RentalId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SupportRequestStatus Status { get; set; } = SupportRequestStatus.Open;
}
