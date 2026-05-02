using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class StorageUnitRepository : InMemoryRepository<StorageUnit>, IStorageUnitRepository
{
    public StorageUnitRepository() : base(MockDatabase.StorageUnits)
    {
    }

    public Task<IReadOnlyList<StorageUnit>> GetByFacilityIdAsync(Guid facilityId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.StorageUnits
                .Where(unit => unit.FacilityId == facilityId)
                .OrderBy(unit => unit.UnitNumber)
                .ToList();

            return Task.FromResult<IReadOnlyList<StorageUnit>>(result);
        }
    }

    public Task<IReadOnlyList<StorageUnit>> GetAvailableUnitsAsync(Guid? facilityId = null)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.StorageUnits
                .Where(unit => unit.Status == StorageUnitStatus.Available)
                .Where(unit => !facilityId.HasValue || unit.FacilityId == facilityId.Value)
                .OrderBy(unit => unit.MonthlyPrice)
                .ToList();

            return Task.FromResult<IReadOnlyList<StorageUnit>>(result);
        }
    }
}
