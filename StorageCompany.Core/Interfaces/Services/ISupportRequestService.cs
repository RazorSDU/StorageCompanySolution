using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Interfaces.Services;

public interface ISupportRequestService
{
    Task<SupportRequest> CreateAsync(Guid customerId, Guid? rentalId, string subject, string message);
    Task<SupportRequest> GetByIdAsync(Guid id);
    Task<IReadOnlyList<SupportRequest>> GetAllAsync();
    Task<IReadOnlyList<SupportRequest>> GetByCustomerIdAsync(Guid customerId);
    Task<SupportRequest> UpdateStatusAsync(Guid id, SupportRequestStatus status);
}
