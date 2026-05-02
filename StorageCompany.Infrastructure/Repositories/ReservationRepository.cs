using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class ReservationRepository : InMemoryRepository<Reservation>, IReservationRepository
{
    public ReservationRepository() : base(MockDatabase.Reservations)
    {
    }

    public Task<IReadOnlyList<Reservation>> GetByCustomerIdAsync(Guid customerId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Reservations
                .Where(reservation => reservation.CustomerId == customerId)
                .OrderByDescending(reservation => reservation.CreatedAtUtc)
                .ToList();

            return Task.FromResult<IReadOnlyList<Reservation>>(result);
        }
    }

    public Task<Reservation?> GetActiveReservationForUnitAsync(Guid storageUnitId)
    {
        lock (MockDatabase.SyncRoot)
        {
            var result = MockDatabase.Reservations
                .FirstOrDefault(reservation => reservation.StorageUnitId == storageUnitId
                                               && reservation.Status is ReservationStatus.Pending or ReservationStatus.Confirmed);

            return Task.FromResult(result);
        }
    }
}
