using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IStorageUnitService
{
    Task<IReadOnlyList<StorageUnit>> GetAllAsync();
    Task<StorageUnit> GetByIdAsync(Guid id);
    Task<IReadOnlyList<StorageUnit>> GetAvailableAsync(Guid? facilityId = null, Guid? unitTypeId = null, decimal? maxPrice = null);
    Task<IReadOnlyList<StorageUnit>> GetByFacilityIdAsync(Guid facilityId);
}
