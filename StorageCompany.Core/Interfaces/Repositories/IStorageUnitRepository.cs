using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IStorageUnitRepository : IRepository<StorageUnit>
{
    Task<IReadOnlyList<StorageUnit>> GetByFacilityIdAsync(Guid facilityId);
    Task<IReadOnlyList<StorageUnit>> GetAvailableUnitsAsync(Guid? facilityId = null);
}
