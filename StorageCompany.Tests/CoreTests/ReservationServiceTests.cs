using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Services;
using StorageCompany.Infrastructure.Data;
using StorageCompany.Infrastructure.Repositories;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box tests for ReservationService.CreateAsync() and ReservationService.CancelAsync().
///
/// CreateAsync — CC = 8:
///   Paths 1–8  : DD-path tests
///   Paths 9–11 : Boundary value tests on move-in date
///
/// CancelAsync — CC = 3:
///   Paths 1–3  : DD-path tests
///   Paths 4–7  : Condition coverage tests
/// </summary>
[Collection("MockDatabase")]
public class ReservationServiceTests
{
    private readonly ReservationService _sut;

    public ReservationServiceTests()
    {
        _sut = new ReservationService(
            new CustomerRepository(),
            new StorageUnitRepository(),
            new ReservationRepository());

        ResetState();
    }

    private static void ResetState()
    {
        lock (MockDatabase.SyncRoot)
        {
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status = StorageUnitStatus.Available;
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitCphMedium).Status   = StorageUnitStatus.Rented;
            MockDatabase.Customers.First(c => c.Id == MockDatabase.Ids.CustomerPeter).IsActive    = true;
            MockDatabase.Reservations.Clear();
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // CreateAsync — DD-path tests (Paths 1–8)
    // ══════════════════════════════════════════════════════════════════

    // Path 1: Customer ID is empty
    [Fact]
    public async Task CreateAsync_WhenCustomerIdIsEmpty_ThrowsBusinessRuleException()
    {
        // Arrange
        var customerId = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(
                customerId,
                MockDatabase.Ids.UnitAarhusSmall,
                DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 2: Storage unit ID is empty
    [Fact]
    public async Task CreateAsync_WhenStorageUnitIdIsEmpty_ThrowsBusinessRuleException()
    {
        // Arrange
        var storageUnitId = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(
                MockDatabase.Ids.CustomerPeter,
                storageUnitId,
                DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 3: Customer is not found
    [Fact]
    public async Task CreateAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(
                nonExistentCustomerId,
                MockDatabase.Ids.UnitAarhusSmall,
                DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 4: Customer is inactive
    [Fact]
    public async Task CreateAsync_WhenCustomerIsInactive_ThrowsBusinessRuleException()
    {
        // Arrange
        MockDatabase.Customers.First(c => c.Id == MockDatabase.Ids.CustomerPeter).IsActive = false;

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(
                MockDatabase.Ids.CustomerPeter,
                MockDatabase.Ids.UnitAarhusSmall,
                DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 5: Storage unit is not found
    [Fact]
    public async Task CreateAsync_WhenStorageUnitDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentUnitId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(
                MockDatabase.Ids.CustomerPeter,
                nonExistentUnitId,
                DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 6: Storage unit is not available
    [Fact]
    public async Task CreateAsync_WhenStorageUnitIsNotAvailable_ThrowsBusinessRuleException()
    {
        // Arrange
        // UnitCphMedium is seeded as Rented

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(
                MockDatabase.Ids.CustomerPeter,
                MockDatabase.Ids.UnitCphMedium,
                DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 7: Move-in date is in the past
    [Fact]
    public async Task CreateAsync_WhenMoveInDateIsInThePast_ThrowsBusinessRuleException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.Date.AddDays(-1);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(
                MockDatabase.Ids.CustomerPeter,
                MockDatabase.Ids.UnitAarhusSmall,
                pastDate));
    }

    // Path 8: Reservation is successfully created
    [Fact]
    public async Task CreateAsync_WhenAllInputIsValid_ReturnsConfirmedReservationAndSetsUnitToReserved()
    {
        // Arrange
        var moveInDate = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            moveInDate);

        // Assert
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);

        var unit = MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall);
        Assert.Equal(StorageUnitStatus.Reserved, unit.Status);
    }

    // ══════════════════════════════════════════════════════════════════
    // CreateAsync — Boundary value tests on move-in date
    // Boundary: moveInDateUtc.Date < DateTime.UtcNow.Date
    // → strictly less than today throws; today and future do not
    // ══════════════════════════════════════════════════════════════════

    // Move-in date is lower than today's date → throws
    [Fact]
    public async Task CreateAsync_WhenMoveInDateIsYesterday_ThrowsBusinessRuleException()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(
                MockDatabase.Ids.CustomerPeter,
                MockDatabase.Ids.UnitAarhusSmall,
                yesterday));
    }

    // Move-in date is equal to today's date → succeeds (boundary: < is strict, so today passes)
    [Fact]
    public async Task CreateAsync_WhenMoveInDateIsToday_Succeeds()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        // Act
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            today);

        // Assert
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    // Move-in date is higher than today's date → succeeds
    [Fact]
    public async Task CreateAsync_WhenMoveInDateIsInTheFuture_Succeeds()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(7);

        // Act
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            futureDate);

        // Assert
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    // ══════════════════════════════════════════════════════════════════
    // CancelAsync — DD-path tests (Paths 1–3)
    // ══════════════════════════════════════════════════════════════════

    // Path 1: Reservation is already cancelled or expired — early return, nothing changes
    [Fact]
    public async Task CancelAsync_WhenReservationIsAlreadyCancelledOrExpired_ReturnsImmediatelyWithoutChanges()
    {
        // Arrange
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            DateTime.UtcNow.Date.AddDays(1));

        reservation.Status = ReservationStatus.Cancelled;
        var unitStatusBefore = MockDatabase.StorageUnits
            .First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status;

        // Act
        await _sut.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(unitStatusBefore, MockDatabase.StorageUnits
            .First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status);
    }

    // Path 2: Reservation is cancellable and the unit is still reserved — both get updated
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedAndUnitIsReserved_CancelsAndFreesUnit()
    {
        // Arrange
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            DateTime.UtcNow.Date.AddDays(1));

        // Act
        await _sut.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(StorageUnitStatus.Available,
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status);
    }

    // Path 3: Reservation is cancellable but the unit is not released
    // (unit exists but status is not Reserved — the if-guard evaluates to false)
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedButUnitIsNotReserved_CancelsWithoutChangingUnit()
    {
        // Arrange
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            DateTime.UtcNow.Date.AddDays(1));

        // Simulate unit having been changed to a non-Reserved status before cancel runs
        MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status
            = StorageUnitStatus.Rented;

        // Act
        await _sut.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        // Unit must NOT have been reset — it was Rented, and the if-guard was false
        Assert.Equal(StorageUnitStatus.Rented,
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status);
    }

    // ══════════════════════════════════════════════════════════════════
    // CancelAsync — Condition coverage
    // The if-condition is: status is Cancelled or Expired (compound or)
    //   → test each sub-condition individually
    // The second if is: unit is not null && unit.Status == Reserved (compound and)
    //   → test unit == null and unit.Status != Reserved separately
    // ══════════════════════════════════════════════════════════════════

    // Reservation status is Cancelled → early return
    [Fact]
    public async Task CancelAsync_WhenReservationStatusIsCancelled_ReturnsImmediately()
    {
        // Arrange
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            DateTime.UtcNow.Date.AddDays(1));

        reservation.Status = ReservationStatus.Cancelled;

        // Act
        await _sut.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    // Reservation status is Expired → early return
    [Fact]
    public async Task CancelAsync_WhenReservationStatusIsExpired_ReturnsImmediately()
    {
        // Arrange
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            DateTime.UtcNow.Date.AddDays(1));

        reservation.Status = ReservationStatus.Expired;

        // Act
        await _sut.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Expired, reservation.Status);
    }

    // Reservation is cancellable, but the storage unit does not exist
    // (unit is null → first part of && is false → unit not freed)
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedAndUnitDoesNotExist_CancelsWithoutFreeing()
    {
        // Arrange
        // Inject a reservation pointing to a unit ID that does not exist in MockDatabase
        var fakeReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CustomerId = MockDatabase.Ids.CustomerPeter,
            StorageUnitId = Guid.NewGuid(), // non-existent unit
            ReservationDateUtc = DateTime.UtcNow,
            MoveInDateUtc = DateTime.UtcNow.Date.AddDays(1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            Status = ReservationStatus.Confirmed,
            CreatedAtUtc = DateTime.UtcNow
        };

        lock (MockDatabase.SyncRoot)
            MockDatabase.Reservations.Add(fakeReservation);

        // Act
        await _sut.CancelAsync(fakeReservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, fakeReservation.Status);
    }

    // Reservation is cancellable, and the storage unit exists, but its status is not Reserved
    // (unit is not null but status != Reserved → second part of && is false → unit not freed)
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedAndUnitExistsButIsNotReserved_CancelsWithoutChangingUnit()
    {
        // Arrange
        var reservation = await _sut.CreateAsync(
            MockDatabase.Ids.CustomerPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            DateTime.UtcNow.Date.AddDays(1));

        // Move unit away from Reserved status
        MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status
            = StorageUnitStatus.Available;

        // Act
        await _sut.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        // Unit was Available before cancel, and must still be Available (not changed)
        Assert.Equal(StorageUnitStatus.Available,
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status);
    }
}
