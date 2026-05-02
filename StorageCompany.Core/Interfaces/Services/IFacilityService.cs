using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IFacilityService
{
    Task<IReadOnlyList<Facility>> GetAllAsync(string? searchTerm = null);
    Task<Facility> GetByIdAsync(Guid id);
}
