using Moq;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Services;
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
///
/// IMPORTANT: Add Moq to StorageCompany.Tests.csproj:
///   <PackageReference Include="Moq" Version="4.20.70" />
/// </summary>
public class ReservationServiceTests
{
    private readonly Mock<ICustomerRepository>     _customerRepo;
    private readonly Mock<IStorageUnitRepository>  _storageUnitRepo;
    private readonly Mock<IReservationRepository>  _reservationRepo;
    private readonly ReservationService            _sut;

    public ReservationServiceTests()
    {
        _customerRepo     = new Mock<ICustomerRepository>();
        _storageUnitRepo  = new Mock<IStorageUnitRepository>();
        _reservationRepo  = new Mock<IReservationRepository>();

        _sut = new ReservationService(
            _customerRepo.Object,
            _storageUnitRepo.Object,
            _reservationRepo.Object);
    }

    // ── Test data builders ────────────────────────────────────────────
    private static Customer ActiveCustomer() => new() { IsActive = true };
    private static Customer InactiveCustomer() => new() { IsActive = false };
    private static StorageUnit AvailableUnit() => new() { Status = StorageUnitStatus.Available };
    private static StorageUnit RentedUnit() => new() { Status = StorageUnitStatus.Rented };
    private static StorageUnit ReservedUnit() => new() { Status = StorageUnitStatus.Reserved };

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
            _sut.CreateAsync(customerId, Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 2: Storage unit ID is empty
    [Fact]
    public async Task CreateAsync_WhenStorageUnitIdIsEmpty_ThrowsBusinessRuleException()
    {
        // Arrange
        var storageUnitId = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), storageUnitId, DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 3: Customer is not found
    [Fact]
    public async Task CreateAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 4: Customer is inactive
    [Fact]
    public async Task CreateAsync_WhenCustomerIsInactive_ThrowsBusinessRuleException()
    {
        // Arrange
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(InactiveCustomer());

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 5: Storage unit is not found
    [Fact]
    public async Task CreateAsync_WhenStorageUnitDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ActiveCustomer());

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((StorageUnit?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 6: Storage unit is not available
    [Fact]
    public async Task CreateAsync_WhenStorageUnitIsNotAvailable_ThrowsBusinessRuleException()
    {
        // Arrange
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ActiveCustomer());

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(RentedUnit());

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1)));
    }

    // Path 7: Move-in date is in the past
    [Fact]
    public async Task CreateAsync_WhenMoveInDateIsInThePast_ThrowsBusinessRuleException()
    {
        // Arrange
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ActiveCustomer());

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(AvailableUnit());

        var pastDate = DateTime.UtcNow.Date.AddDays(-1);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), pastDate));
    }

    // Path 8: Reservation is successfully created
    [Fact]
    public async Task CreateAsync_WhenAllInputIsValid_ReturnsConfirmedReservationAndSetsUnitToReserved()
    {
        // Arrange
        var unit = AvailableUnit();

        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ActiveCustomer());

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(unit);

        var futureDate = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var reservation = await _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), futureDate);

        // Assert
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
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
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ActiveCustomer());

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(AvailableUnit());

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), yesterday));
    }

    // Move-in date is equal to today's date → succeeds (boundary: < is strict, so today passes)
    [Fact]
    public async Task CreateAsync_WhenMoveInDateIsToday_Succeeds()
    {
        // Arrange
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ActiveCustomer());

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(AvailableUnit());

        var today = DateTime.UtcNow.Date;

        // Act
        var reservation = await _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), today);

        // Assert
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    // Move-in date is higher than today's date → succeeds
    [Fact]
    public async Task CreateAsync_WhenMoveInDateIsInTheFuture_Succeeds()
    {
        // Arrange
        _customerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ActiveCustomer());

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(AvailableUnit());

        var futureDate = DateTime.UtcNow.Date.AddDays(7);

        // Act
        var reservation = await _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), futureDate);

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
        var reservation = new Reservation { Status = ReservationStatus.Cancelled };

        _reservationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(reservation);

        // Act
        await _sut.CancelAsync(Guid.NewGuid());

        // Assert — status unchanged and UpdateAsync was never called
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        _reservationRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    // Path 2: Reservation is cancellable and the unit is still reserved — both get updated
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedAndUnitIsReserved_CancelsAndFreesUnit()
    {
        // Arrange
        var unit = ReservedUnit();
        var reservation = new Reservation
        {
            Status        = ReservationStatus.Confirmed,
            StorageUnitId = Guid.NewGuid()
        };

        _reservationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(reservation);

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync(unit);

        // Act
        await _sut.CancelAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(StorageUnitStatus.Available, unit.Status);
    }

    // Path 3: Reservation is cancellable but the unit is not released
    // (unit exists but status is not Reserved — the if-guard evaluates to false)
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedButUnitIsNotReserved_CancelsWithoutChangingUnit()
    {
        // Arrange
        var unit = AvailableUnit(); // not Reserved
        var reservation = new Reservation
        {
            Status        = ReservationStatus.Confirmed,
            StorageUnitId = Guid.NewGuid()
        };

        _reservationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(reservation);

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync(unit);

        // Act
        await _sut.CancelAsync(Guid.NewGuid());

        // Assert — reservation cancelled, unit unchanged
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(StorageUnitStatus.Available, unit.Status);
        _storageUnitRepo.Verify(r => r.UpdateAsync(It.IsAny<StorageUnit>()), Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════
    // CancelAsync — Condition coverage
    // Condition 1: status is Cancelled or Expired (compound or)
    //   → test each sub-condition individually
    // Condition 2: unit is not null && unit.Status == Reserved (compound and)
    //   → test unit == null and unit.Status != Reserved separately
    // ══════════════════════════════════════════════════════════════════

    // Reservation status is Cancelled → early return
    [Fact]
    public async Task CancelAsync_WhenReservationStatusIsCancelled_ReturnsImmediately()
    {
        // Arrange
        var reservation = new Reservation { Status = ReservationStatus.Cancelled };

        _reservationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(reservation);

        // Act
        await _sut.CancelAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        _reservationRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    // Reservation status is Expired → early return
    [Fact]
    public async Task CancelAsync_WhenReservationStatusIsExpired_ReturnsImmediately()
    {
        // Arrange
        var reservation = new Reservation { Status = ReservationStatus.Expired };

        _reservationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(reservation);

        // Act
        await _sut.CancelAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(ReservationStatus.Expired, reservation.Status);
        _reservationRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    // Reservation is cancellable, but the storage unit does not exist
    // (unit is null → first part of && is false → unit not freed)
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedAndUnitDoesNotExist_CancelsWithoutFreeing()
    {
        // Arrange
        var reservation = new Reservation
        {
            Status        = ReservationStatus.Confirmed,
            StorageUnitId = Guid.NewGuid()
        };

        _reservationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(reservation);

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync((StorageUnit?)null);

        // Act
        await _sut.CancelAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        _storageUnitRepo.Verify(r => r.UpdateAsync(It.IsAny<StorageUnit>()), Times.Never);
    }

    // Reservation is cancellable, and the storage unit exists, but its status is not Reserved
    // (unit is not null but status != Reserved → second part of && is false → unit not freed)
    [Fact]
    public async Task CancelAsync_WhenReservationIsConfirmedAndUnitExistsButIsNotReserved_CancelsWithoutChangingUnit()
    {
        // Arrange
        var unit = AvailableUnit();
        var reservation = new Reservation
        {
            Status        = ReservationStatus.Confirmed,
            StorageUnitId = Guid.NewGuid()
        };

        _reservationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(reservation);

        _storageUnitRepo
            .Setup(r => r.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync(unit);

        // Act
        await _sut.CancelAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        _storageUnitRepo.Verify(r => r.UpdateAsync(It.IsAny<StorageUnit>()), Times.Never);
    }
}
