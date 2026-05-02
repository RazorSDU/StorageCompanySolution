using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IReservationService
{
    Task<Reservation> CreateAsync(Guid customerId, Guid storageUnitId, DateTime moveInDateUtc);
    Task<Reservation> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Reservation>> GetByCustomerIdAsync(Guid customerId);
    Task CancelAsync(Guid id);
}
