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
/// These tests cover:
/// - empty customer id,
/// - empty storage unit id,
/// - customer not found,
/// - inactive customer,
/// - storage unit not found,
/// - storage unit unavailable,
/// - move-in date before today,
/// - move-in date equal to today,
/// - move-in date after today,
/// - success path including expiry date and unit status update.
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

    [Fact]
    public async Task CreateAsync_CustomerIdIsEmpty_ThrowsBusinessRuleExceptionAndDoesNotCallRepositories()
    {
        // Arrange
        var customerId = Guid.Empty;
        var storageUnitId = Guid.NewGuid();
        var moveInDateUtc = DateTime.UtcNow.Date;

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, moveInDateUtc));

        // Assert
        Assert.Contains("customerId", exception.Message);

        VerifyNoCustomerLookup();
        VerifyNoStorageUnitLookup();
        VerifyNoReservationCreated();
        VerifyNoStorageUnitStateChange();
    }

    [Fact]
    public async Task CreateAsync_StorageUnitIdIsEmpty_ThrowsBusinessRuleExceptionAndDoesNotCallRepositories()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.Empty;
        var moveInDateUtc = DateTime.UtcNow.Date;

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, moveInDateUtc));

        // Assert
        Assert.Contains("storageUnitId", exception.Message);

        VerifyNoCustomerLookup();
        VerifyNoStorageUnitLookup();
        VerifyNoReservationCreated();
        VerifyNoStorageUnitStateChange();
    }

    [Fact]
    public async Task CreateAsync_CustomerDoesNotExist_ThrowsNotFoundExceptionAndDoesNotChangeUnitState()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Contains("Customer", exception.Message);

        _customerRepositoryMock.Verify(
            repository => repository.GetByIdAsync(customerId),
            Times.Once);

        VerifyNoStorageUnitLookup();
        VerifyNoReservationCreated();
        VerifyNoStorageUnitStateChange();
    }

    [Fact]
    public async Task CreateAsync_CustomerIsInactive_ThrowsBusinessRuleExceptionAndDoesNotChangeUnitState()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var inactiveCustomer = CreateCustomer(
            id: customerId,
            isActive: false);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(inactiveCustomer);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Equal("Inactive customers cannot create reservations.", exception.Message);

        _customerRepositoryMock.Verify(
            repository => repository.GetByIdAsync(customerId),
            Times.Once);

        VerifyNoStorageUnitLookup();
        VerifyNoReservationCreated();
        VerifyNoStorageUnitStateChange();
    }

    [Fact]
    public async Task CreateAsync_StorageUnitDoesNotExist_ThrowsNotFoundExceptionAndDoesNotChangeUnitState()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var activeCustomer = CreateCustomer(
            id: customerId,
            isActive: true);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync((StorageUnit?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Contains("Storage unit", exception.Message);

        _customerRepositoryMock.Verify(
            repository => repository.GetByIdAsync(customerId),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(storageUnitId),
            Times.Once);

        VerifyNoReservationCreated();
        VerifyNoStorageUnitStateChange();
    }

    [Theory]
    [InlineData(StorageUnitStatus.Reserved)]
    [InlineData(StorageUnitStatus.Rented)]
    [InlineData(StorageUnitStatus.Maintenance)]
    public async Task CreateAsync_StorageUnitIsNotAvailable_ThrowsBusinessRuleExceptionAndDoesNotChangeUnitState(
        StorageUnitStatus unavailableStatus)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var activeCustomer = CreateCustomer(
            id: customerId,
            isActive: true);

        var unavailableUnit = CreateStorageUnit(
            id: storageUnitId,
            status: unavailableStatus);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync(unavailableUnit);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Equal("This storage unit is not available.", exception.Message);
        Assert.Equal(unavailableStatus, unavailableUnit.Status);

        VerifyNoReservationCreated();
        VerifyNoStorageUnitStateChange();
    }

    [Fact]
    public async Task CreateAsync_MoveInDateIsBeforeToday_ThrowsBusinessRuleExceptionAndStorageUnitRemainsAvailable()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var activeCustomer = CreateCustomer(
            id: customerId,
            isActive: true);

        var availableUnit = CreateStorageUnit(
            id: storageUnitId,
            status: StorageUnitStatus.Available);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync(availableUnit);

        var moveInDateBeforeToday = DateTime.UtcNow.Date.AddDays(-1);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateAsync(customerId, storageUnitId, moveInDateBeforeToday));

        // Assert
        Assert.Equal("Move-in date cannot be in the past.", exception.Message);
        Assert.Equal(StorageUnitStatus.Available, availableUnit.Status);

        VerifyNoReservationCreated();
        VerifyNoStorageUnitStateChange();
    }

    [Fact]
    public async Task CreateAsync_MoveInDateIsToday_CreatesReservationWithExpiryDateAndReservesUnit()
    {
        // Arrange
        var moveInDateToday = DateTime.UtcNow.Date;

        var testData = ArrangeValidCreateScenario(
            moveInDateUtc: moveInDateToday);

        // Act
        var result = await _service.CreateAsync(
            testData.CustomerId,
            testData.StorageUnitId,
            moveInDateToday);

        // Assert
        AssertSuccessfulReservation(
            result,
            testData.CustomerId,
            testData.StorageUnitId,
            moveInDateToday,
            testData.StorageUnit);
    }

    [Fact]
    public async Task CreateAsync_MoveInDateIsAfterToday_CreatesReservationWithExpiryDateAndReservesUnit()
    {
        // Arrange
        var moveInDateAfterToday = DateTime.UtcNow.Date.AddDays(1);

        var testData = ArrangeValidCreateScenario(
            moveInDateUtc: moveInDateAfterToday);

        // Act
        var result = await _service.CreateAsync(
            testData.CustomerId,
            testData.StorageUnitId,
            moveInDateAfterToday);

        // Assert
        AssertSuccessfulReservation(
            result,
            testData.CustomerId,
            testData.StorageUnitId,
            moveInDateAfterToday,
            testData.StorageUnit);
    }

    private ValidCreateScenario ArrangeValidCreateScenario(DateTime moveInDateUtc)
    {
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var activeCustomer = CreateCustomer(
            id: customerId,
            isActive: true);

        var availableUnit = CreateStorageUnit(
            id: storageUnitId,
            status: StorageUnitStatus.Available);

        _customerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(customerId))
            .ReturnsAsync(activeCustomer);

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetByIdAsync(storageUnitId))
            .ReturnsAsync(availableUnit);

        _reservationRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _storageUnitRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<StorageUnit>()))
            .Returns(Task.CompletedTask);

        return new ValidCreateScenario(
            customerId,
            storageUnitId,
            moveInDateUtc,
            availableUnit);
    }

    private void AssertSuccessfulReservation(
        Reservation result,
        Guid expectedCustomerId,
        Guid expectedStorageUnitId,
        DateTime expectedMoveInDateUtc,
        StorageUnit storageUnit)
    {
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(expectedCustomerId, result.CustomerId);
        Assert.Equal(expectedStorageUnitId, result.StorageUnitId);
        Assert.Equal(ReservationStatus.Confirmed, result.Status);

        Assert.Equal(
            DateTime.SpecifyKind(expectedMoveInDateUtc, DateTimeKind.Utc),
            result.MoveInDateUtc);

        Assert.Equal(DateTimeKind.Utc, result.MoveInDateUtc.Kind);

        Assert.NotNull(result.ExpiresAtUtc);

        var expiresAtUtc = result.ExpiresAtUtc.Value;

        Assert.True(
            expiresAtUtc > result.ReservationDateUtc,
            "The reservation should be given an expiry date after the reservation date.");

        Assert.InRange(
            (expiresAtUtc - result.ReservationDateUtc).TotalDays,
            6.99,
            7.01);

        Assert.Equal(StorageUnitStatus.Reserved, storageUnit.Status);

        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.Is<Reservation>(
                reservation =>
                    reservation.CustomerId == expectedCustomerId &&
                    reservation.StorageUnitId == expectedStorageUnitId &&
                    reservation.Status == ReservationStatus.Confirmed)),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<StorageUnit>(
                unit =>
                    unit.Id == expectedStorageUnitId &&
                    unit.Status == StorageUnitStatus.Reserved)),
            Times.Once);
    }

    private void VerifyNoCustomerLookup()
    {
        _customerRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    private void VerifyNoStorageUnitLookup()
    {
        _storageUnitRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    private void VerifyNoReservationCreated()
    {
        _reservationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Reservation>()),
            Times.Never);
    }

    private void VerifyNoStorageUnitStateChange()
    {
        _storageUnitRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
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

    private sealed record ValidCreateScenario(
        Guid CustomerId,
        Guid StorageUnitId,
        DateTime MoveInDateUtc,
        StorageUnit StorageUnit);
}