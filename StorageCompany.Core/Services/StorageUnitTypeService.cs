using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class StorageUnitTypeService : IStorageUnitTypeService
{
    private readonly IStorageUnitTypeRepository _unitTypes;

    public StorageUnitTypeService(IStorageUnitTypeRepository unitTypes)
    {
        _unitTypes = unitTypes;
    }

    public Task<IReadOnlyList<StorageUnitType>> GetAllAsync() => _unitTypes.GetAllAsync();

    public async Task<StorageUnitType> GetByIdAsync(Guid id)
    {
        var unitType = await _unitTypes.GetByIdAsync(id);
        return unitType ?? throw new NotFoundException($"Storage unit type '{id}' was not found.");
    }
}
