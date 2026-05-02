using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class RentalRepository : InMemoryRepository<Rental>, IRentalRepository
{
    public RentalRepository() : base(MockDatabase.Rentals)
    {
    }

    public Task<IReadOnlyList<Rental>> GetByCustomerIdAsync(Guid customerId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Rentals
                .Where(rental => rental.CustomerId == customerId)
                .OrderByDescending(rental => rental.CreatedAtUtc)
                .ToList();

            return Task.FromResult<IReadOnlyList<Rental>>(result);
        }
    }

    public Task<Rental?> GetActiveRentalForUnitAsync(Guid storageUnitId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Rentals
                .FirstOrDefault(rental => rental.StorageUnitId == storageUnitId && rental.Status == RentalStatus.Active);

            return Task.FromResult(result);
        }
    }
}
