using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Repositories;

namespace StorageCompany.Specs.Fakes;

public class FakeStorageUnitRepository(List<StorageUnit> storageUnits) : IStorageUnitRepository
{
    private readonly List<StorageUnit> _storageUnits = storageUnits;

    public Task<StorageUnit?> GetByIdAsync(Guid id) =>
        Task.FromResult(_storageUnits.FirstOrDefault(u => u.Id == id));

    public Task AddAsync(StorageUnit storageUnit) { _storageUnits.Add(storageUnit); return Task.CompletedTask; }

    public Task UpdateAsync(StorageUnit storageUnit)
    {
        var index = _storageUnits.FindIndex(u => u.Id == storageUnit.Id);
        if (index >= 0) _storageUnits[index] = storageUnit;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id) { _storageUnits.RemoveAll(u => u.Id == id); return Task.CompletedTask; }

    public Task<IReadOnlyList<StorageUnit>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<StorageUnit>>([.. _storageUnits]);

    public Task<IReadOnlyList<StorageUnit>> GetByFacilityIdAsync(Guid facilityId) =>
        Task.FromResult<IReadOnlyList<StorageUnit>>(
            [.. _storageUnits.Where(u => u.FacilityId == facilityId).OrderBy(u => u.UnitNumber)]);

    public Task<IReadOnlyList<StorageUnit>> GetAvailableUnitsAsync(Guid? facilityId = null) =>
        Task.FromResult<IReadOnlyList<StorageUnit>>(
            [.. _storageUnits.Where(u => u.Status == StorageUnitStatus.Available)
                  .Where(u => !facilityId.HasValue || u.FacilityId == facilityId.Value)
                  .OrderBy(u => u.MonthlyPrice)]);
}
