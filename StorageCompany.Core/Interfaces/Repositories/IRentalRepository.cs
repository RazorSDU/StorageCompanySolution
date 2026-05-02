using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IRentalRepository : IRepository<Rental>
{
    Task<IReadOnlyList<Rental>> GetByCustomerIdAsync(Guid customerId);
    Task<Rental?> GetActiveRentalForUnitAsync(Guid storageUnitId);
}
