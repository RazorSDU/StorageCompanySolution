using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IFacilityRepository : IRepository<Facility>
{
    Task<IReadOnlyList<Facility>> SearchByCityOrPostalCodeAsync(string searchTerm);
}
