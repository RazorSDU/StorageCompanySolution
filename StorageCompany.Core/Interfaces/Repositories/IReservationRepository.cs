using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IReadOnlyList<Reservation>> GetByCustomerIdAsync(Guid customerId);
    Task<Reservation?> GetActiveReservationForUnitAsync(Guid storageUnitId);
}
