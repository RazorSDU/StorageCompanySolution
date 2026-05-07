using Moq;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Services;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box derived unit tests for ReservationService.CreateAsync.
/// 
/// Method under test:
/// CreateAsync(Guid customerId, Guid storageUnitId, DateTime moveInDateUtc)
///
/// The tests cover validation paths, not-found paths, business-rule paths,
/// and the successful path where a reservation is created and the storage unit is reserved.
/// </summary>
public class ReservationServiceCreateWhiteBoxTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IStorageUnitRepository> _storageUnitRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly ReservationService _service;

    public ReservationServiceCreateWhiteBoxTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _storageUnitRepositoryMock = new Mock<IStorageUnitRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();

        _service = new ReservationService(
            _customerRepositoryMock.Object,
            _storageUnitRepositoryMock.Object,
            _reservationRepositoryMock.Object);
    }

    [Theory]
    [InlineData("customerId")]
    [InlineData("storageUnitId")]
    public async Task CreateAsync_RequiredGuidIsEmpty_ThrowsBusinessRuleExceptionAndDoesNotCallRepositories(
        string emptyParameter)
    {
        // Arrange
        var customerId = emptyParameter == "customerId" ? Guid.Empty : Guid.NewGuid();
        var storageUnitId = emptyParameter == "storageUnitId" ? Guid.Empty : Guid.NewGuid();
        var moveInDateUtc = DateTime.UtcNow.AddDays(1);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, moveInDateUtc));

        // Assert
        Assert.Contains("cannot be empty", exception.Message);

        _customerRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CustomerDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.AddDays(1)));

        // Assert
        Assert.Contains("Customer", exception.Message);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CustomerIsInactive_ThrowsBusinessRuleException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var inactiveCustomer = CreateCustomer(customerId, isActive: false);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(inactiveCustomer);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.AddDays(1)));

        // Assert
        Assert.Equal("Inactive customers cannot create reservations.", exception.Message);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_StorageUnitDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var activeCustomer = CreateCustomer(customerId, isActive: true);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync((StorageUnit?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.AddDays(1)));

        // Assert
        Assert.Contains("Storage unit", exception.Message);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Never);
    }

    [Theory]
    [InlineData(StorageUnitStatus.Reserved)]
    [InlineData(StorageUnitStatus.Rented)]
    [InlineData(StorageUnitStatus.Maintenance)]
    public async Task CreateAsync_StorageUnitIsNotAvailable_ThrowsBusinessRuleException(
        StorageUnitStatus status)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var activeCustomer = CreateCustomer(customerId, isActive: true);
        var unavailableUnit = CreateStorageUnit(storageUnitId, status);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync(unavailableUnit);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.AddDays(1)));

        // Assert
        Assert.Equal("This storage unit is not available.", exception.Message);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_MoveInDateIsInPast_ThrowsBusinessRuleException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var activeCustomer = CreateCustomer(customerId, isActive: true);
        var availableUnit = CreateStorageUnit(storageUnitId, StorageUnitStatus.Available);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync(availableUnit);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.AddDays(-1)));

        // Assert
        Assert.Equal("Move-in date cannot be in the past.", exception.Message);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_AddsReservationAndMarksStorageUnitAsReserved()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();
        var moveInDateUtc = DateTime.UtcNow.AddDays(1);

        var activeCustomer = CreateCustomer(customerId, isActive: true);
        var availableUnit = CreateStorageUnit(storageUnitId, StorageUnitStatus.Available);

        Reservation? addedReservation = null;
        StorageUnit? updatedUnit = null;

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync(availableUnit);

        _reservationRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(reservation => addedReservation = reservation)
            .Returns(Task.CompletedTask);

        _storageUnitRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<StorageUnit>()))
            .Callback<StorageUnit>(unit => updatedUnit = unit)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(customerId, storageUnitId, moveInDateUtc);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(storageUnitId, result.StorageUnitId);
        Assert.Equal(ReservationStatus.Confirmed, result.Status);
        Assert.Equal(DateTimeKind.Utc, result.MoveInDateUtc.Kind);

        Assert.NotNull(addedReservation);
        Assert.Same(result, addedReservation);

        Assert.NotNull(updatedUnit);
        Assert.Equal(StorageUnitStatus.Reserved, updatedUnit.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Once);
    }

    private static Customer CreateCustomer(Guid id, bool isActive)
    {
        return new Customer
        {
            Id = id,
            FirstName = "Test",
            LastName = "Customer",
            Email = "test@example.com",
            PhoneNumber = "12345678",
            PasswordHash = "not-relevant-for-this-unit-test",
            IsActive = isActive,
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