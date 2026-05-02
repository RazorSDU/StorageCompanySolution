using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class StorageUnitTypeRepository : InMemoryRepository<StorageUnitType>, IStorageUnitTypeRepository
{
    public StorageUnitTypeRepository() : base(MockDatabase.StorageUnitTypes)
    {
    }
}
