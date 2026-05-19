using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class ReservationService : IReservationService
{
    private readonly IUserRepository _users;
    private readonly IStorageUnitRepository _storageUnits;
    private readonly IReservationRepository _reservations;

    public ReservationService(
        IUserRepository users,
        IStorageUnitRepository storageUnits,
        IReservationRepository reservations)
    {
        _users = users;
        _storageUnits = storageUnits;
        _reservations = reservations;
    }

    public async Task<Reservation> CreateAsync(Guid customerId, Guid storageUnitId, DateTime moveInDateUtc)
    {
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstEmpty(storageUnitId, nameof(storageUnitId));

        var customer = await _users.GetByIdAsync(customerId)
            ?? throw new NotFoundException($"User '{customerId}' was not found.");

        if (!customer.IsActive)
            throw new BusinessRuleException("Inactive users cannot create reservations.");

        var unit = await _storageUnits.GetByIdAsync(storageUnitId)
            ?? throw new NotFoundException($"Storage unit '{storageUnitId}' was not found.");

        if (unit.Status != StorageUnitStatus.Available)
            throw new BusinessRuleException("This storage unit is not available.");

        if (moveInDateUtc.Date < DateTime.UtcNow.Date)
            throw new BusinessRuleException("Move-in date cannot be in the past.");

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            StorageUnitId = storageUnitId,
            ReservationDateUtc = DateTime.UtcNow,
            MoveInDateUtc = DateTime.SpecifyKind(moveInDateUtc, DateTimeKind.Utc),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            Status = ReservationStatus.Confirmed,
            CreatedAtUtc = DateTime.UtcNow
        };

        unit.Status = StorageUnitStatus.Reserved;

        await _reservations.AddAsync(reservation);
        await _storageUnits.UpdateAsync(unit);

        return reservation;
    }

    public async Task<Reservation> GetByIdAsync(Guid id)
    {
        var reservation = await _reservations.GetByIdAsync(id);
        return reservation ?? throw new NotFoundException($"Reservation '{id}' was not found.");
    }

    public Task<IReadOnlyList<Reservation>> GetByCustomerIdAsync(Guid customerId)
    {
        return _reservations.GetByCustomerIdAsync(customerId);
    }

    public async Task CancelAsync(Guid id)
    {
        var reservation = await GetByIdAsync(id);

        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Expired)
            return;

        reservation.Status = ReservationStatus.Cancelled;
        await _reservations.UpdateAsync(reservation);

        var unit = await _storageUnits.GetByIdAsync(reservation.StorageUnitId);
        if (unit is not null && unit.Status == StorageUnitStatus.Reserved)
        {
            unit.Status = StorageUnitStatus.Available;
            await _storageUnits.UpdateAsync(unit);
        }
    }
}
