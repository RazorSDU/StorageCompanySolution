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
/// White-box derived unit tests for RentalService.EndRentalAsync.
///
/// Method under test:
/// EndRentalAsync(Guid id, DateTime endDateUtc)
///
/// These tests cover:
/// - rental not found,
/// - rental is not active (Ended),
/// - rental is not active (Cancelled),
/// - rental is not active (Overdue),
/// - end date is before start date,
/// - end date is equal to start date (success),
/// - end date is after start date (success),
/// - storage unit status is set to Available on success,
/// - storage unit not found does not block rental from ending,
/// - access code is deactivated on success.
/// </summary>
public class RentalServiceEndRentalWhiteBoxTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<IStorageUnitRepository> _storageUnitRepositoryMock;
    private readonly Mock<IAccessCodeService> _accessCodeServiceMock;
    private readonly RentalService _service;

    public RentalServiceEndRentalWhiteBoxTests()
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

    // Test 21
    // White-box: Første beslutningspunkt i EndRentalAsync er om rental eksisterer.
    // Hvis GetByIdAsync returnerer null, kastes NotFoundException.
    // Vi verificerer at UpdateAsync aldrig kaldes, da vi stopper tidligt.
    [Fact]
    public async Task EndRentalAsync_RentalDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        // Vi opretter et tilfældigt rental-id som IKKE findes i repository.
        var rentalId = Guid.NewGuid();

        _rentalRepositoryMock
            .Setup(r => r.GetByIdAsync(rentalId))
            .ReturnsAsync((Rental?)null); // Simulerer at rental ikke eksisterer

        // Act
        // Vi forventer at metoden kaster NotFoundException
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.EndRentalAsync(rentalId, DateTime.UtcNow.Date));

        // Assert
        // Fejlbeskeden skal nævne "Rental" så kalderen ved hvad der mangler
        Assert.Contains("Rental", exception.Message);

        // Ingen opdatering må ske, da vi aldrig nåede til den logik
        _rentalRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 22
    // White-box: Andet beslutningspunkt er om rental har status Active.
    // En rental med status Ended er allerede afsluttet og må ikke afsluttes igen.
    // Vi tester den ene gren: status == Ended → BusinessRuleException.
    [Fact]
    public async Task EndRentalAsync_RentalStatusIsEnded_ThrowsBusinessRuleException()
    {
        // Arrange
        // Vi opretter en rental der allerede er afsluttet (Ended)
        var rentalId = Guid.NewGuid();
        var rental = CreateRental(id: rentalId, status: RentalStatus.Ended, startDate: DateTime.UtcNow.Date.AddDays(-10));

        _rentalRepositoryMock
            .Setup(r => r.GetByIdAsync(rentalId))
            .ReturnsAsync(rental);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.EndRentalAsync(rentalId, DateTime.UtcNow.Date));

        // Assert
        // Fejlbeskeden skal præcist matche hvad koden kaster
        Assert.Equal("Only active rentals can be ended.", exception.Message);

        // Ingen opdatering må ske - vi kastet exception inden vi kom dertil
        _rentalRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 23
    // White-box: Same beslutningspunkt som test 22, men med status Cancelled.
    // Boundary testing: vi tester alle mulige ikke-aktive statuser separat.
    [Fact]
    public async Task EndRentalAsync_RentalStatusIsCancelled_ThrowsBusinessRuleException()
    {
        // Arrange
        // En annulleret rental må heller ikke afsluttes
        var rentalId = Guid.NewGuid();
        var rental = CreateRental(id: rentalId, status: RentalStatus.Cancelled, startDate: DateTime.UtcNow.Date.AddDays(-5));

        _rentalRepositoryMock
            .Setup(r => r.GetByIdAsync(rentalId))
            .ReturnsAsync(rental);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.EndRentalAsync(rentalId, DateTime.UtcNow.Date));

        // Assert
        Assert.Equal("Only active rentals can be ended.", exception.Message);

        _rentalRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 24
    // White-box: Same beslutningspunkt, nu med status Overdue.
    // Vi dækker alle grene af "er rental aktiv?"-betingelsen.
    [Fact]
    public async Task EndRentalAsync_RentalStatusIsOverdue_ThrowsBusinessRuleException()
    {
        // Arrange
        // En overskredet rental er ikke Active og må ikke afsluttes via denne metode
        var rentalId = Guid.NewGuid();
        var rental = CreateRental(id: rentalId, status: RentalStatus.Overdue, startDate: DateTime.UtcNow.Date.AddDays(-30));

        _rentalRepositoryMock
            .Setup(r => r.GetByIdAsync(rentalId))
            .ReturnsAsync(rental);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.EndRentalAsync(rentalId, DateTime.UtcNow.Date));

        // Assert
        Assert.Equal("Only active rentals can be ended.", exception.Message);

        _rentalRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 25
    // White-box: Tredje beslutningspunkt er datovalidering.
    // Slutdato må ikke være før startdato - dette er en forretningsregel.
    // Vi tester boundary: én dag FØR startdato → fejl.
    [Fact]
    public async Task EndRentalAsync_EndDateIsBeforeStartDate_ThrowsBusinessRuleException()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var rental = CreateRental(id: rentalId, status: RentalStatus.Active, startDate: startDate);

        _rentalRepositoryMock
            .Setup(r => r.GetByIdAsync(rentalId))
            .ReturnsAsync(rental);

        // Boundary: én dag INDEN startdato
        var endDateBeforeStart = startDate.AddDays(-1);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.EndRentalAsync(rentalId, endDateBeforeStart));

        // Assert
        Assert.Equal("End date cannot be before the rental start date.", exception.Message);

        // Ingen opdatering sker, vi er kastte ud af metoden inden da
        _rentalRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Rental>()),
            Times.Never);
    }

    // Test 26
    // White-box: Success-path med boundary-dato (slutdato == startdato).
    // En rental der starter og slutter samme dag skal være gyldig.
    // Vi verificerer at status, EndDateUtc og unit-status alle sættes korrekt.
    [Fact]
    public async Task EndRentalAsync_EndDateEqualsStartDate_EndsRentalSuccessfully()
    {
        // Arrange
        // Boundary: slutdato er præcis lig startdato (minimum gyldig)
        var startDate = DateTime.UtcNow.Date.AddDays(-1);
        var endDate = startDate;

        var (rentalId, storageUnitId, rental, storageUnit) = ArrangeValidEndRentalScenario(startDate);

        // Act
        var result = await _service.EndRentalAsync(rentalId, endDate);

        // Assert
        // Fælles assertion-metode verificerer alle success-betingelser
        AssertSuccessfulEndRental(result, rentalId, endDate, storageUnit);
    }

    // Test 27
    // White-box: Success-path med slutdato EFTER startdato (normal brug).
    // Vi tester den anden boundary: flere dage imellem start og slut.
    [Fact]
    public async Task EndRentalAsync_EndDateIsAfterStartDate_EndsRentalSuccessfully()
    {
        // Arrange
        // Normal scenarie: rental løber over 10 dage
        var startDate = DateTime.UtcNow.Date.AddDays(-10);
        var endDate = DateTime.UtcNow.Date;

        var (rentalId, storageUnitId, rental, storageUnit) = ArrangeValidEndRentalScenario(startDate);

        // Act
        var result = await _service.EndRentalAsync(rentalId, endDate);

        // Assert
        AssertSuccessfulEndRental(result, rentalId, endDate, storageUnit);
    }

    // Test 28
    // White-box: Side-effect test - når en rental afsluttes skal storage unit-status
    // sættes tilbage til Available, så enheden kan udlejes igen.
    // Vi verificerer BÅDE at objektet ændres OG at UpdateAsync kaldes med korrekte værdier.
    [Fact]
    public async Task EndRentalAsync_StorageUnitExists_SetsUnitStatusToAvailable()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date.AddDays(-5);
        var endDate = DateTime.UtcNow.Date;

        var (rentalId, storageUnitId, rental, storageUnit) = ArrangeValidEndRentalScenario(startDate);

        // Act
        await _service.EndRentalAsync(rentalId, endDate);

        // Assert
        // In-memory objektet skal have fået ny status
        Assert.Equal(StorageUnitStatus.Available, storageUnit.Status);

        // Repository skal have fået kaldt UpdateAsync med den korrekte enhed
        _storageUnitRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<StorageUnit>(u =>
                u.Id == storageUnitId &&
                u.Status == StorageUnitStatus.Available)),
            Times.Once);
    }

    // Test 29
    // White-box: Defensiv gren - hvis storage unit ikke længere eksisterer i databasen
    // (fx slettet manuelt), skal rentalen stadig afsluttes uden at kaste fejl.
    // Vi tester at GetByIdAsync returnerer null for unit, og at UpdateAsync IKKE kaldes for unit.
    [Fact]
    public async Task EndRentalAsync_StorageUnitDoesNotExist_StillEndsRentalWithoutError()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(-5);
        var endDate = DateTime.UtcNow.Date;

        var rental = CreateRental(id: rentalId, status: RentalStatus.Active, startDate: startDate);
        rental.StorageUnitId = storageUnitId;

        _rentalRepositoryMock
            .Setup(r => r.GetByIdAsync(rentalId))
            .ReturnsAsync(rental);

        // Simulerer at storage unit ikke kan findes (fx slettet fra databasen)
        _storageUnitRepositoryMock
            .Setup(r => r.GetByIdAsync(storageUnitId))
            .ReturnsAsync((StorageUnit?)null);

        _rentalRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Rental>()))
            .Returns(Task.CompletedTask);

        _accessCodeServiceMock
            .Setup(s => s.DeactivateByRentalIdAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.EndRentalAsync(rentalId, endDate);

        // Assert
        // Rental skal stadig afsluttes korrekt
        Assert.Equal(RentalStatus.Ended, result.Status);
        Assert.NotNull(result.EndDateUtc);

        // Storage unit UpdateAsync må IKKE kaldes, da vi ikke har en enhed at opdatere
        _storageUnitRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<StorageUnit>()),
            Times.Never);
    }

    // Test 30
    // White-box: Side-effect test - access code skal deaktiveres når rental afsluttes,
    // så lejeren ikke længere kan tilgå enheden.
    // Vi verificerer at DeactivateByRentalIdAsync kaldes præcis én gang med korrekt id.
    [Fact]
    public async Task EndRentalAsync_ActiveRental_DeactivatesAccessCode()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date.AddDays(-3);
        var endDate = DateTime.UtcNow.Date;

        var (rentalId, _, _, _) = ArrangeValidEndRentalScenario(startDate);

        // Act
        await _service.EndRentalAsync(rentalId, endDate);

        // Assert
        // Access code service skal have kaldt DeactivateByRentalIdAsync med det specifikke rental id
        _accessCodeServiceMock.Verify(
            s => s.DeactivateByRentalIdAsync(rentalId),
            Times.Once);
    }

    private (Guid RentalId, Guid StorageUnitId, Rental Rental, StorageUnit StorageUnit)
        ArrangeValidEndRentalScenario(DateTime startDate)
    {
        var rentalId = Guid.NewGuid();
        var storageUnitId = Guid.NewGuid();

        var rental = CreateRental(id: rentalId, status: RentalStatus.Active, startDate: startDate);
        rental.StorageUnitId = storageUnitId;

        var storageUnit = new StorageUnit
        {
            Id = storageUnitId,
            FacilityId = Guid.NewGuid(),
            UnitTypeId = Guid.NewGuid(),
            UnitNumber = "C-303",
            Floor = 3,
            MonthlyPrice = 2000m,
            Status = StorageUnitStatus.Rented,
            IsClimateControlled = false,
            IsDriveUp = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _rentalRepositoryMock
            .Setup(r => r.GetByIdAsync(rentalId))
            .ReturnsAsync(rental);

        _storageUnitRepositoryMock
            .Setup(r => r.GetByIdAsync(storageUnitId))
            .ReturnsAsync(storageUnit);

        _storageUnitRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<StorageUnit>()))
            .Returns(Task.CompletedTask);

        _accessCodeServiceMock
            .Setup(s => s.DeactivateByRentalIdAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _rentalRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Rental>()))
            .Returns(Task.CompletedTask);

        return (rentalId, storageUnitId, rental, storageUnit);
    }

    private void AssertSuccessfulEndRental(
        Rental result,
        Guid expectedRentalId,
        DateTime expectedEndDate,
        StorageUnit storageUnit)
    {
        Assert.Equal(expectedRentalId, result.Id);
        Assert.Equal(RentalStatus.Ended, result.Status);
        Assert.NotNull(result.EndDateUtc);
        Assert.Equal(
            DateTime.SpecifyKind(expectedEndDate, DateTimeKind.Utc),
            result.EndDateUtc!.Value);
        Assert.Equal(DateTimeKind.Utc, result.EndDateUtc.Value.Kind);
        Assert.Equal(StorageUnitStatus.Available, storageUnit.Status);

        _rentalRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Rental>(rental =>
                rental.Id == expectedRentalId &&
                rental.Status == RentalStatus.Ended)),
            Times.Once);
    }

    private static Rental CreateRental(Guid id, RentalStatus status, DateTime startDate) => new Rental
    {
        Id = id,
        CustomerId = Guid.NewGuid(),
        StorageUnitId = Guid.NewGuid(),
        StartDateUtc = startDate,
        MonthlyPrice = 1000m,
        Status = status,
        CreatedAtUtc = DateTime.UtcNow
    };
}

