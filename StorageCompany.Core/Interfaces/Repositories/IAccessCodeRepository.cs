using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IAccessCodeRepository : IRepository<AccessCode>
{
    Task<AccessCode?> GetActiveByRentalIdAsync(Guid rentalId);
}
