using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IRentalService
{
    Task<Rental> CreateFromReservationAsync(Guid reservationId);
    Task<Rental> CreateDirectAsync(Guid customerId, Guid storageUnitId, DateTime startDateUtc);
    Task<Rental> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Rental>> GetByCustomerIdAsync(Guid customerId);
    Task<Rental> EndRentalAsync(Guid id, DateTime endDateUtc);
}
