using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class RentalService : IRentalService
{
    private readonly ICustomerRepository _customers;
    private readonly IReservationRepository _reservations;
    private readonly IRentalRepository _rentals;
    private readonly IStorageUnitRepository _storageUnits;
    private readonly IAccessCodeService _accessCodeService;

    public RentalService(
        ICustomerRepository customers,
        IReservationRepository reservations,
        IRentalRepository rentals,
        IStorageUnitRepository storageUnits,
        IAccessCodeService accessCodeService)
    {
        _customers = customers;
        _reservations = reservations;
        _rentals = rentals;
        _storageUnits = storageUnits;
        _accessCodeService = accessCodeService;
    }

    public async Task<Rental> CreateFromReservationAsync(Guid reservationId)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId)
            ?? throw new NotFoundException($"Reservation '{reservationId}' was not found.");

        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new BusinessRuleException("Cannot convert a cancelled or expired reservation to a rental.");

        var unit = await _storageUnits.GetByIdAsync(reservation.StorageUnitId)
            ?? throw new NotFoundException($"Storage unit '{reservation.StorageUnitId}' was not found.");

        if (unit.Status is not (StorageUnitStatus.Reserved or StorageUnitStatus.Available))
            throw new BusinessRuleException("This storage unit cannot be rented.");

        var activeRental = await _rentals.GetActiveRentalForUnitAsync(unit.Id);
        if (activeRental is not null)
            throw new BusinessRuleException("This storage unit already has an active rental.");

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            CustomerId = reservation.CustomerId,
            StorageUnitId = unit.Id,
            StartDateUtc = reservation.MoveInDateUtc,
            MonthlyPrice = unit.MonthlyPrice,
            Status = RentalStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        reservation.Status = ReservationStatus.Confirmed;
        unit.Status = StorageUnitStatus.Rented;

        await _rentals.AddAsync(rental);
        await _reservations.UpdateAsync(reservation);
        await _storageUnits.UpdateAsync(unit);
        await _accessCodeService.GenerateForRentalAsync(rental.Id);

        return rental;
    }

    public async Task<Rental> CreateDirectAsync(Guid customerId, Guid storageUnitId, DateTime startDateUtc)
    {
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstEmpty(storageUnitId, nameof(storageUnitId));

        var customer = await _customers.GetByIdAsync(customerId)
            ?? throw new NotFoundException($"Customer '{customerId}' was not found.");

        if (!customer.IsActive)
            throw new BusinessRuleException("Inactive customers cannot create rentals.");

        var unit = await _storageUnits.GetByIdAsync(storageUnitId)
            ?? throw new NotFoundException($"Storage unit '{storageUnitId}' was not found.");

        if (unit.Status != StorageUnitStatus.Available)
            throw new BusinessRuleException("This storage unit is not available.");

        if (startDateUtc.Date < DateTime.UtcNow.Date)
            throw new BusinessRuleException("Start date cannot be in the past.");

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            StorageUnitId = storageUnitId,
            StartDateUtc = DateTime.SpecifyKind(startDateUtc, DateTimeKind.Utc),
            MonthlyPrice = unit.MonthlyPrice,
            Status = RentalStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        unit.Status = StorageUnitStatus.Rented;

        await _rentals.AddAsync(rental);
        await _storageUnits.UpdateAsync(unit);
        await _accessCodeService.GenerateForRentalAsync(rental.Id);

        return rental;
    }

    public async Task<Rental> GetByIdAsync(Guid id)
    {
        var rental = await _rentals.GetByIdAsync(id);
        return rental ?? throw new NotFoundException($"Rental '{id}' was not found.");
    }

    public Task<IReadOnlyList<Rental>> GetByCustomerIdAsync(Guid customerId)
    {
        return _rentals.GetByCustomerIdAsync(customerId);
    }

    public async Task<Rental> EndRentalAsync(Guid id, DateTime endDateUtc)
    {
        var rental = await GetByIdAsync(id);

        if (rental.Status != RentalStatus.Active)
            throw new BusinessRuleException("Only active rentals can be ended.");

        if (endDateUtc.Date < rental.StartDateUtc.Date)
            throw new BusinessRuleException("End date cannot be before the rental start date.");

        rental.EndDateUtc = DateTime.SpecifyKind(endDateUtc, DateTimeKind.Utc);
        rental.Status = RentalStatus.Ended;

        var unit = await _storageUnits.GetByIdAsync(rental.StorageUnitId);
        if (unit is not null)
        {
            unit.Status = StorageUnitStatus.Available;
            await _storageUnits.UpdateAsync(unit);
        }

        await _accessCodeService.DeactivateByRentalIdAsync(rental.Id);
        await _rentals.UpdateAsync(rental);
        return rental;
    }
}
