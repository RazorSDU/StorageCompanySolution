using Reqnroll;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Services;
using StorageCompany.Specs.Fakes;
using Xunit;

namespace StorageCompany.Specs.StepDefinitions;

[Binding]
public class CancelAsyncSteps
{
    private readonly List<Reservation> _reservations = [];
    private readonly List<StorageUnit> _storageUnits = [];
    private readonly ReservationService _reservationService;

    private Guid _reservationId;
    private Guid _storageUnitId;

    public CancelAsyncSteps()
    {
        _reservationService = new ReservationService(
            new FakeCustomerRepository([]),
            new FakeStorageUnitRepository(_storageUnits),
            new FakeReservationRepository(_reservations));
    }

    [Given("a reservation with status Cancelled")]
    public void GivenCancelledReservation()
    {
        _storageUnitId = Guid.NewGuid();
        AddReservation(ReservationStatus.Cancelled);
    }

    [Given("a reservation with status Expired")]
    public void GivenExpiredReservation()
    {
        _storageUnitId = Guid.NewGuid();
        AddReservation(ReservationStatus.Expired);
    }

    [Given("an active reservation")]
    public void GivenActiveReservation()
    {
        _storageUnitId = Guid.NewGuid();
        AddReservation(ReservationStatus.Confirmed);
    }

    [Given("the storage unit is reserved")]
    public void GivenStorageUnitIsReserved()
    {
        AddStorageUnit(StorageUnitStatus.Reserved);
    }

    [Given("the storage unit exists but is not reserved")]
    public void GivenStorageUnitExistsNotReserved()
    {
        AddStorageUnit(StorageUnitStatus.Rented);
    }

    [Given("the storage unit does not exist")]
    public void GivenStorageUnitDoesNotExist()
    {
        // No setup needed. The default state has no storage unit in the repository.
        // This step exists to make the precondition explicit in the feature file,
        // keeping A5 consistent with A3 and A4 which both declare their unit condition in Given.
    }

    [When("the customer cancels the reservation")]
    public async Task WhenCustomerCancels()
    {
        await _reservationService.CancelAsync(_reservationId);
    }

    [Then("the reservation status should remain Cancelled")]
    public void ThenStatusRemainsCancelled()
    {
        Assert.Equal(ReservationStatus.Cancelled, GetReservation().Status);
    }

    [Then("the reservation status should remain Expired")]
    public void ThenStatusRemainsExpired()
    {
        Assert.Equal(ReservationStatus.Expired, GetReservation().Status);
    }

    [Then("the reservation should be cancelled")]
    public void ThenReservationIsCancelled()
    {
        Assert.Equal(ReservationStatus.Cancelled, GetReservation().Status);
    }

    [Then("the storage unit should become available")]
    public void ThenStorageUnitBecomesAvailable()
    {
        Assert.Equal(StorageUnitStatus.Available, GetStorageUnit().Status);
    }

    [Then("the storage unit should not become available")]
    public void ThenStorageUnitDoesNotBecomeAvailable()
    {
        Assert.NotEqual(StorageUnitStatus.Available, GetStorageUnit().Status);
    }

    private void AddReservation(ReservationStatus status)
    {
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            StorageUnitId = _storageUnitId,
            MoveInDateUtc = DateTime.UtcNow.AddDays(7),
            Status = status
        };
        _reservations.Add(reservation);
        _reservationId = reservation.Id;
    }

    private void AddStorageUnit(StorageUnitStatus status)
    {
        _storageUnits.Add(new StorageUnit
        {
            Id = _storageUnitId,
            MonthlyPrice = 500,
            Status = status
        });
    }

    private Reservation GetReservation() => _reservations.First(r => r.Id == _reservationId);
    private StorageUnit GetStorageUnit() => _storageUnits.First(u => u.Id == _storageUnitId);
}
