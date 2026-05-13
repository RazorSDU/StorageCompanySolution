using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Repositories;

namespace StorageCompany.Specs.Fakes;

public class FakeReservationRepository(List<Reservation> reservations) : IReservationRepository
{
    private readonly List<Reservation> _reservations = reservations;

    public Task<Reservation?> GetByIdAsync(Guid id) =>
        Task.FromResult(_reservations.FirstOrDefault(r => r.Id == id));

    public Task AddAsync(Reservation reservation) { _reservations.Add(reservation); return Task.CompletedTask; }

    public Task UpdateAsync(Reservation reservation)
    {
        var index = _reservations.FindIndex(r => r.Id == reservation.Id);
        if (index >= 0) _reservations[index] = reservation;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id) { _reservations.RemoveAll(r => r.Id == id); return Task.CompletedTask; }

    public Task<IReadOnlyList<Reservation>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Reservation>>([.. _reservations]);

    public Task<IReadOnlyList<Reservation>> GetByCustomerIdAsync(Guid customerId) =>
        Task.FromResult<IReadOnlyList<Reservation>>(
            [.. _reservations.Where(r => r.CustomerId == customerId).OrderByDescending(r => r.CreatedAtUtc)]);

    public Task<Reservation?> GetActiveReservationForUnitAsync(Guid storageUnitId) =>
        Task.FromResult(
            _reservations.FirstOrDefault(r =>
                r.StorageUnitId == storageUnitId &&
                r.Status is ReservationStatus.Pending or ReservationStatus.Confirmed));
}
