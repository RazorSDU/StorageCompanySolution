using Moq;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Services;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box derived unit tests for ReservationService.CancelAsync.
/// 
/// Method under test:
/// CancelAsync(Guid id)
///
/// The tests cover:
/// 1. Reservation does not exist.
/// 2. Reservation is already Cancelled or Expired.
/// 3. Reservation can be cancelled.
/// 4. Related storage unit is Reserved and therefore made Available.
/// 5. Related storage unit is missing or not Reserved.
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
            repository => repository.UpdateAsync(It.IsAny<Reservation>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Theory]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.Expired)]
    public async Task CancelAsync_ReservationIsAlreadyCancelledOrExpired_ReturnsWithoutUpdating(
        ReservationStatus status)
    {
        // Arrange
        var reservation = CreateReservation(status);

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Reservation>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelAsync_ConfirmedReservationWithReservedStorageUnit_CancelsReservationAndMakesUnitAvailable()
    {
        // Arrange
        var reservation = CreateReservation(ReservationStatus.Confirmed);

        var reservedUnit = new StorageUnit
        {
            Id = reservation.StorageUnitId,
            FacilityId = Guid.NewGuid(),
            UnitTypeId = Guid.NewGuid(),
            UnitNumber = "A-101",
            Floor = 1,
            MonthlyPrice = 1000m,
            Status = StorageUnitStatus.Reserved,
            IsClimateControlled = false,
            IsDriveUp = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        Reservation? updatedReservation = null;
        StorageUnit? updatedStorageUnit = null;

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(updated => updatedReservation = updated)
            .Returns(Task.CompletedTask);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync(reservedUnit);

        _storageUnitRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<StorageUnit>()))
            .Callback<StorageUnit>(updated => updatedStorageUnit = updated)
            .Returns(Task.CompletedTask);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.NotNull(updatedReservation);
        Assert.Equal(ReservationStatus.Cancelled, updatedReservation.Status);

        Assert.NotNull(updatedStorageUnit);
        Assert.Equal(StorageUnitStatus.Available, updatedStorageUnit.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Reservation>()),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ConfirmedReservationButStorageUnitDoesNotExist_CancelsReservationOnly()
    {
        // Arrange
        var reservation = CreateReservation(ReservationStatus.Confirmed);

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync((StorageUnit?)null);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(reservation),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    [Theory]
    [InlineData(StorageUnitStatus.Available)]
    [InlineData(StorageUnitStatus.Rented)]
    [InlineData(StorageUnitStatus.Maintenance)]
    public async Task CancelAsync_ConfirmedReservationButStorageUnitIsNotReserved_CancelsReservationOnly(
        StorageUnitStatus unitStatus)
    {
        // Arrange
        var reservation = CreateReservation(ReservationStatus.Confirmed);

        var unit = new StorageUnit
        {
            Id = reservation.StorageUnitId,
            FacilityId = Guid.NewGuid(),
            UnitTypeId = Guid.NewGuid(),
            UnitNumber = "A-101",
            Floor = 1,
            MonthlyPrice = 1000m,
            Status = unitStatus,
            IsClimateControlled = false,
            IsDriveUp = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _reservationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(reservation.StorageUnitId))
            .ReturnsAsync(unit);

        // Act
        await _service.CancelAsync(reservation.Id);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.UpdateAsync(reservation),
            Times.Once);

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
            MoveInDateUtc = DateTime.UtcNow.AddDays(1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}