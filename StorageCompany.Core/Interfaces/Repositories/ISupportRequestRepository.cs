using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface ISupportRequestRepository : IRepository<SupportRequest>
{
    Task<IReadOnlyList<SupportRequest>> GetByCustomerIdAsync(Guid customerId);
}
