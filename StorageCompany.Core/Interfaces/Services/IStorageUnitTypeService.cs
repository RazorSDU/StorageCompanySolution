using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IStorageUnitTypeService
{
    Task<IReadOnlyList<StorageUnitType>> GetAllAsync();
    Task<StorageUnitType> GetByIdAsync(Guid id);
}
