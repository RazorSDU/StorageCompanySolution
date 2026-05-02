using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class AccessCodeRepository : InMemoryRepository<AccessCode>, IAccessCodeRepository
{
    public AccessCodeRepository() : base(MockDatabase.AccessCodes)
    {
    }

    public Task<AccessCode?> GetActiveByRentalIdAsync(Guid rentalId)
    {
        lock (MockDatabase.SyncRoot)
        {
            return Task.FromResult(MockDatabase.AccessCodes.FirstOrDefault(code => code.RentalId == rentalId && code.IsActive));
        }
    }
}
