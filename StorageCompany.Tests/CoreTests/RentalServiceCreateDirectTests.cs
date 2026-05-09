using Moq;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Services;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box derived unit tests for RentalService.CreateDirectAsync.
///
/// Method under test:
/// CreateDirectAsync(Guid customerId, Guid storageUnitId, DateTime startDateUtc)
///
/// These tests cover:
/// - empty customer id,
/// - empty storage unit id,
/// - customer not found,
/// - inactive customer,
/// - storage unit not found,
/// - storage unit not available,
/// - start date before today,
/// - start date equal to today (success),
/// - start date after today (success).
/// </summary>
public class RentalServiceCreateDirectWhiteBoxTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<IStorageUnitRepository> _storageUnitRepositoryMock;
    private readonly Mock<IAccessCodeService> _accessCodeServiceMock;
    private readonly RentalService _service;

    public RentalServiceCreateDirectWhiteBoxTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _rentalRepositoryMock = new Mock<IRentalRepository>();
        _storageUnitRepositoryMock = new Mock<IStorageUnitRepository>();
        _accessCodeServiceMock = new Mock<IAccessCodeService>();

        _service = new RentalService(
            _customerRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _rentalRepositoryMock.Object,
            _storageUnitRepositoryMock.Object,
            _accessCodeServiceMock.Object);
    }

    // Test 12
    // White-box: CreateDirectAsync starter med to Guard-kald der validerer at id'erne ikke er tomme.
    // Hvis customerId er Guid.Empty kastes BusinessRuleException straks - ingen repository-kald.
    [Fact]
    public async Task CreateDirectAsync_CustomerIdIsEmpty_ThrowsBusinessRuleExceptionAndDoesNotCallRepositories()
    {
        // Arrange
        // Guid.Empty er den ugyldige "tom" Guid-værdi (00000000-0000-...)
        var customerId = Guid.Empty;
        var storageUnitId = Guid.NewGuid();
        var startDateUtc = DateTime.UtcNow.Date;

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateDirectAsync(customerId, storageUnitId, startDateUtc));

        // Assert
        // Fejlbeskeden skal indeholde "customerId" så det er tydeligt hvilket argument der fejlede
        Assert.Contains("customerId", exception.Message);

        // Ingen repository-kald skal ske - vi kastede exception inden metodens logik startede
        _customerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 13
    // White-box: Anden Guard-validering - storageUnitId må ikke være tom.
    // Vi tester at begge Guard-kald er til stede og returnerer fejl ved tomme værdier.
    [Fact]
    public async Task CreateDirectAsync_StorageUnitIdIsEmpty_ThrowsBusinessRuleExceptionAndDoesNotCallRepositories()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.Empty; // Ugyldig tom Guid
        var startDateUtc = DateTime.UtcNow.Date;

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateDirectAsync(customerId, storageUnitId, startDateUtc));

        // Assert
        Assert.Contains("storageUnitId", exception.Message);

        _customerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 14
    // White-box: Første repository-kald er GetByIdAsync på customer.
    // Hvis customer ikke findes kastes NotFoundException.
    // Vi verificerer at storage unit IKKE søges op - metoden stopper ved første fejl.
    [Fact]
    public async Task CreateDirectAsync_CustomerDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        // Simulerer at customer ikke eksisterer i databasen
        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateDirectAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Contains("Customer", exception.Message);

        _customerRepositoryMock.Verify(
            r => r.GetByIdAsync(customerId),
            Times.Once);

        // Storage unit søges IKKE op - vi stoppede ved customer-fejlen
        _storageUnitRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 15
    // White-box: Beslutningspunkt - er customer aktiv?
    // Kun aktive kunder må oprette rentals. Inaktiv → BusinessRuleException.
    // Storage unit søges ikke op, da vi allerede har fejlet.
    [Fact]
    public async Task CreateDirectAsync_CustomerIsInactive_ThrowsBusinessRuleException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        // Customer eksisterer, men er markeret som inaktiv
        var inactiveCustomer = CreateCustomer(id: customerId, isActive: false);

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(inactiveCustomer);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateDirectAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Equal("Inactive customers cannot create rentals.", exception.Message);

        // Storage unit repository må ikke kontaktes
        _storageUnitRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 16
    // White-box: Storage unit-opslag - hvis enheden ikke eksisterer kastes NotFoundException.
    // Vi verificerer at customer ALLEREDE er slået op, men rental ALDRIG oprettes.
    [Fact]
    public async Task CreateDirectAsync_StorageUnitDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(CreateCustomer(id: customerId, isActive: true));

        // Enheden eksisterer ikke
        _storageUnitRepositoryMock
            .Setup(r => r.GetByIdAsync(storageUnitId))
            .ReturnsAsync((StorageUnit?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateDirectAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Contains("Storage unit", exception.Message);

        _storageUnitRepositoryMock.Verify(
            r => r.GetByIdAsync(storageUnitId),
            Times.Once);

        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 17
    // White-box: Beslutningspunkt - er storage unit Available?
    // Kun enheder med status Available kan lejes direkte.
    // Vi tester alle tre ikke-ledige statuser via [Theory] med [InlineData].
    [Theory]
    [InlineData(StorageUnitStatus.Reserved)]   // Reserveret af en anden kunde
    [InlineData(StorageUnitStatus.Rented)]     // Allerede udlejet
    [InlineData(StorageUnitStatus.Maintenance)] // Under vedligeholdelse
    public async Task CreateDirectAsync_StorageUnitIsNotAvailable_ThrowsBusinessRuleException(
        StorageUnitStatus unavailableStatus)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(CreateCustomer(id: customerId, isActive: true));

        // Enheden eksisterer men er ikke Available
        _storageUnitRepositoryMock
            .Setup(r => r.GetByIdAsync(storageUnitId))
            .ReturnsAsync(CreateStorageUnit(id: storageUnitId, status: unavailableStatus));

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateDirectAsync(customerId, storageUnitId, DateTime.UtcNow.Date));

        // Assert
        Assert.Equal("This storage unit is not available.", exception.Message);

        // Ingen rental oprettes og enheden opdateres ikke
        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Rental>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    // Test 18
    // White-box: Datovalidering - startdato må ikke være i fortiden.
    // Boundary: én dag FØR i dag → BusinessRuleException.
    // Vi verificerer at enheden forbliver Available (ingen side-effects).
    [Fact]
    public async Task CreateDirectAsync_StartDateIsBeforeToday_ThrowsBusinessRuleExceptionAndUnitRemainsAvailable()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();
        var availableUnit = CreateStorageUnit(id: storageUnitId, status: StorageUnitStatus.Available);

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(CreateCustomer(id: customerId, isActive: true));

        _storageUnitRepositoryMock
            .Setup(r => r.GetByIdAsync(storageUnitId))
            .ReturnsAsync(availableUnit);

        // Boundary: præcis én dag tilbage i tid
        var startDateBeforeToday = DateTime.UtcNow.Date.AddDays(-1);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.CreateDirectAsync(customerId, storageUnitId, startDateBeforeToday));

        // Assert
        Assert.Equal("Start date cannot be in the past.", exception.Message);

        // Enheden skal IKKE have ændret status - vi kastede exception inden
        Assert.Equal(StorageUnitStatus.Available, availableUnit.Status);

        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Rental>()),
            Times.Never);

        _storageUnitRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    // Test 19
    // White-box: Success-path med boundary-dato (startdato == i dag).
    // Vi verificerer alle success-betingelser: rental oprettes, unit sættes til Rented,
    // access code genereres, og alle felter på rental er korrekte.
    [Fact]
    public async Task CreateDirectAsync_StartDateIsToday_CreatesRentalAndSetsUnitToRented()
    {
        // Arrange
        // Boundary: præcis dagens dato - den mindst mulige gyldige dato
        var startDateToday = DateTime.UtcNow.Date;
        var (customerId, storageUnitId, availableUnit) = ArrangeValidDirectRentalScenario();

        // Act
        var result = await _service.CreateDirectAsync(customerId, storageUnitId, startDateToday);

        // Assert
        // Fælles assertion-metode tjekker alle aspekter af success-resultatet
        AssertSuccessfulDirectRental(result, customerId, storageUnitId, startDateToday, availableUnit);
    }

    // Test 20
    // White-box: Success-path med startdato EFTER i dag (fremtidig).
    // Vi tester at fremtidige datoer også er gyldige og at resultatet er korrekt.
    [Fact]
    public async Task CreateDirectAsync_StartDateIsAfterToday_CreatesRentalAndSetsUnitToRented()
    {
        // Arrange
        // Fremtidig startdato - kunden planlægger at flytte ind om 5 dage
        var startDateAfterToday = DateTime.UtcNow.Date.AddDays(5);
        var (customerId, storageUnitId, availableUnit) = ArrangeValidDirectRentalScenario();

        // Act
        var result = await _service.CreateDirectAsync(customerId, storageUnitId, startDateAfterToday);

        // Assert
        AssertSuccessfulDirectRental(result, customerId, storageUnitId, startDateAfterToday, availableUnit);
    }

    private (Guid CustomerId, Guid StorageUnitId, StorageUnit StorageUnit) ArrangeValidDirectRentalScenario()
    {
        var customerId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();
        var availableUnit = CreateStorageUnit(id: storageUnitId, status: StorageUnitStatus.Available);

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(CreateCustomer(id: customerId, isActive: true));

        _storageUnitRepositoryMock
            .Setup(r => r.GetByIdAsync(storageUnitId))
            .ReturnsAsync(availableUnit);

        _rentalRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Rental>()))
            .Returns(Task.CompletedTask);

        _storageUnitRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<StorageUnit>()))
            .Returns(Task.CompletedTask);

        _accessCodeServiceMock
            .Setup(s => s.GenerateForRentalAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new AccessCode());

        return (customerId, storageUnitId, availableUnit);
    }

    private void AssertSuccessfulDirectRental(
        Rental result,
        Guid expectedCustomerId,
        Guid expectedStorageUnitId,
        DateTime expectedStartDate,
        StorageUnit storageUnit)
    {
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(expectedCustomerId, result.CustomerId);
        Assert.Equal(expectedStorageUnitId, result.StorageUnitId);
        Assert.Equal(RentalStatus.Active, result.Status);
        Assert.Equal(DateTime.SpecifyKind(expectedStartDate, DateTimeKind.Utc), result.StartDateUtc);
        Assert.Equal(DateTimeKind.Utc, result.StartDateUtc.Kind);
        Assert.Equal(StorageUnitStatus.Rented, storageUnit.Status);

        _rentalRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Rental>(rental =>
                rental.CustomerId == expectedCustomerId &&
                rental.StorageUnitId == expectedStorageUnitId &&
                rental.Status == RentalStatus.Active)),
            Times.Once);

        _storageUnitRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<StorageUnit>(unit =>
                unit.Id == expectedStorageUnitId &&
                unit.Status == StorageUnitStatus.Rented)),
            Times.Once);

        _accessCodeServiceMock.Verify(
            s => s.GenerateForRentalAsync(result.Id),
            Times.Once);
    }

    private static Customer CreateCustomer(Guid id, bool isActive) => new Customer
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

    private static StorageUnit CreateStorageUnit(Guid id, StorageUnitStatus status) => new StorageUnit
    {
        Id = id,
        FacilityId = Guid.NewGuid(),
        UnitTypeId = Guid.NewGuid(),
        UnitNumber = "B-202",
        Floor = 2,
        MonthlyPrice = 1500m,
        Status = status,
        IsClimateControlled = false,
        IsDriveUp = false,
        CreatedAtUtc = DateTime.UtcNow
    };
}

