using Moq;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Services;
using Xunit;

namespace StorageCompany.Tests.CoreTests.WhiteBox;

/// <summary>
/// White-box derived unit tests for ReservationService.CancelAsync.
///
/// Method under test:
/// CancelAsync(Guid id)
///
/// These tests cover:
/// - reservation not found,
/// - reservation already Cancelled,
/// - reservation already Expired,
/// - cancellable reservation with reserved storage unit,
/// - cancellable reservation where storage unit does not exist,
/// - cancellable reservation where storage unit exists but is not Reserved.
/// </summary>
public class ReservationServiceCancelWhiteBoxTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IStorageUnitRepository> _storageUnitRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly ReservationService _service;

    public ReservationServiceCancelWhiteBoxTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _storageUnitRepositoryMock = new Mock<IStorageUnitRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();

        _service = new ReservationService(
            _customerRepositoryMock.Object,
            _storageUnitRepositoryMock.Object,
            _reservationRepositoryMock.Object);
    }

    [Fact]
    public async Task CancelAsync_ReservationDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservationId))
            .ReturnsAsync((Reservation?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CancelAsync(reservationId));

        // Assert
        Assert.Contains("Reservation", exception.Message);

        _reservationRepositoryMock.Verify(
            repository => repository.GetByIdAsync(reservationId),
            Times.Once);

        VerifyReservationWasNotUpdated();
        VerifyStorageUnitWasNotLookedUp();
        VerifyStorageUnitWasNotUpdated();
    }

    [Fact]
    public async Task CancelAsync_ReservationStatusIsCancelled_ReturnsImmediatelyAndStatusRemainsCancelled()
    {
        // Arrange
        var reservation = CreateReservation(
            status: ReservationStatus.Cancelled);

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);

        VerifyReservationWasNotUpdated();
        VerifyStorageUnitWasNotLookedUp();
        VerifyStorageUnitWasNotUpdated();
    }

    [Fact]
    public async Task CancelAsync_ReservationStatusIsExpired_ReturnsImmediatelyAndStatusRemainsExpired()
    {
        // Arrange
        var reservation = CreateReservation(
            status: ReservationStatus.Expired);

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Expired, reservation.Status);

        VerifyReservationWasNotUpdated();
        VerifyStorageUnitWasNotLookedUp();
        VerifyStorageUnitWasNotUpdated();
    }

    [Fact]
    public async Task CancelAsync_ConfirmedReservationWithReservedStorageUnit_CancelsReservationAndMakesUnitAvailable()
    {
        // Arrange
        var reservation = CreateReservation(
            status: ReservationStatus.Confirmed);

        var reservedUnit = CreateStorageUnit(
            id: reservation.StorageUnitId,
            status: StorageUnitStatus.Reserved);

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync(reservedUnit);

        _storageUnitRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<StorageUnit>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(StorageUnitStatus.Available, reservedUnit.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<Reservation>(
                updatedReservation =>
                    updatedReservation.Id == reservation.Id &&
                    updatedReservation.Status == ReservationStatus.Cancelled)),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<StorageUnit>(
                updatedUnit =>
                    updatedUnit.Id == reservedUnit.Id &&
                    updatedUnit.Status == StorageUnitStatus.Available)),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ConfirmedReservationButStorageUnitDoesNotExist_CancelsReservationOnly()
    {
        // Arrange
        var reservation = CreateReservation(
            status: ReservationStatus.Confirmed);

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync((StorageUnit?)null);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<Reservation>(
                updatedReservation =>
                    updatedReservation.Id == reservation.Id &&
                    updatedReservation.Status == ReservationStatus.Cancelled)),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(reservation.StorageUnitId),
            Times.Once);

        VerifyStorageUnitWasNotUpdated();
    }

    [Theory]
    [InlineData(StorageUnitStatus.Available)]
    [InlineData(StorageUnitStatus.Rented)]
    [InlineData(StorageUnitStatus.Maintenance)]
    public async Task CancelAsync_ConfirmedReservationButStorageUnitIsNotReserved_CancelsReservationOnly(
        StorageUnitStatus unitStatus)
    {
        // Arrange
        var reservation = CreateReservation(
            status: ReservationStatus.Confirmed);

        var unit = CreateStorageUnit(
            id: reservation.StorageUnitId,
            status: unitStatus);

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync(unit);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(unitStatus, unit.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<Reservation>(
                updatedReservation =>
                    updatedReservation.Id == reservation.Id &&
                    updatedReservation.Status == ReservationStatus.Cancelled)),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(reservation.StorageUnitId),
            Times.Once);

        VerifyStorageUnitWasNotUpdated();
    }

    private void VerifyReservationWasNotUpdated()
    {
        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Reservation>()),
            Times.Never);
    }

    private void VerifyStorageUnitWasNotLookedUp()
    {
        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    private void VerifyStorageUnitWasNotUpdated()
    {
        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    private static Reservation CreateReservation(ReservationStatus status)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            StorageUnitId = Guid.NewGuid(),
            ReservationDateUtc = DateTime.UtcNow,
            MoveInDateUtc = DateTime.UtcNow.Date.AddDays(1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static StorageUnit CreateStorageUnit(Guid id, StorageUnitStatus status)
    {
        return new StorageUnit
        {
            Id = id,
            FacilityId = Guid.NewGuid(),
            UnitTypeId = Guid.NewGuid(),
            UnitNumber = "A-101",
            Floor = 1,
            MonthlyPrice = 1000m,
            Status = status,
            IsClimateControlled = false,
            IsDriveUp = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}