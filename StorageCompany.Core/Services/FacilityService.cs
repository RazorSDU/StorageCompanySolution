using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _facilities;

    public FacilityService(IFacilityRepository facilities)
    {
        _facilities = facilities;
    }

    public Task<IReadOnlyList<Facility>> GetAllAsync(string? searchTerm = null)
    {
        return string.IsNullOrWhiteSpace(searchTerm)
            ? _facilities.GetAllAsync()
            : _facilities.SearchByCityOrPostalCodeAsync(searchTerm);
    }

    public async Task<Facility> GetByIdAsync(Guid id)
    {
        var facility = await _facilities.GetByIdAsync(id);
        return facility ?? throw new NotFoundException($"Facility '{id}' was not found.");
    }
}
