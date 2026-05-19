using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class SupportRequestRepository : InMemoryRepository<SupportRequest>, ISupportRequestRepository
{
    public SupportRequestRepository() : base(MockDatabase.SupportRequests)
    {
    }

    public Task<IReadOnlyList<SupportRequest>> GetByCustomerIdAsync(Guid customerId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.SupportRequests
                .Where(request => request.UserId == customerId)
                .OrderByDescending(request => request.CreatedAtUtc)
                .ToList();

            return Task.FromResult<IReadOnlyList<SupportRequest>>(result);
        }
    }
}
