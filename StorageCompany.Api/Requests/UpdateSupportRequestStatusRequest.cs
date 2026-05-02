using StorageCompany.Core.Enums;

namespace StorageCompany.Api.Requests;

public class UpdateSupportRequestStatusRequest
{
    public SupportRequestStatus Status { get; set; }
}
