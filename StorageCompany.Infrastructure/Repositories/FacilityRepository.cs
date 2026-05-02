using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class FacilityRepository : InMemoryRepository<Facility>, IFacilityRepository
{
    public FacilityRepository() : base(MockDatabase.Facilities)
    {
    }

    public Task<IReadOnlyList<Facility>> SearchByCityOrPostalCodeAsync(string searchTerm)
    {
        lock (MockDatabase.SyncRoot)
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            var result = MockDatabase.Facilities
                .Where(f => f.City.ToLowerInvariant().Contains(normalized)
                            || f.PostalCode.ToLowerInvariant().Contains(normalized)
                            || f.Name.ToLowerInvariant().Contains(normalized))
                .OrderBy(f => f.City)
                .ToList();

            return Task.FromResult<IReadOnlyList<Facility>>(result);
        }
    }
}
