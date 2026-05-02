using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IAccessCodeService
{
    Task<AccessCode> GenerateForRentalAsync(Guid rentalId);
    Task<AccessCode> GetActiveByRentalIdAsync(Guid rentalId);
    Task DeactivateByRentalIdAsync(Guid rentalId);
}
