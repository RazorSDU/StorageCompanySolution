using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class StorageUnitService : IStorageUnitService
{
    private readonly IStorageUnitRepository _storageUnits;

    public StorageUnitService(IStorageUnitRepository storageUnits)
    {
        _storageUnits = storageUnits;
    }

    public Task<IReadOnlyList<StorageUnit>> GetAllAsync() => _storageUnits.GetAllAsync();

    public async Task<StorageUnit> GetByIdAsync(Guid id)
    {
        var unit = await _storageUnits.GetByIdAsync(id);
        return unit ?? throw new NotFoundException($"Storage unit '{id}' was not found.");
    }

    public async Task<IReadOnlyList<StorageUnit>> GetAvailableAsync(Guid? facilityId = null, Guid? unitTypeId = null, decimal? maxPrice = null)
    {
        var units = await _storageUnits.GetAvailableUnitsAsync(facilityId);

        if (unitTypeId.HasValue)
            units = units.Where(unit => unit.UnitTypeId == unitTypeId.Value).ToList();

        if (maxPrice.HasValue)
            units = units.Where(unit => unit.MonthlyPrice <= maxPrice.Value).ToList();

        return units;
    }

    public Task<IReadOnlyList<StorageUnit>> GetByFacilityIdAsync(Guid facilityId)
    {
        return _storageUnits.GetByFacilityIdAsync(facilityId);
    }
}
