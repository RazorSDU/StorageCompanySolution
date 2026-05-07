using Moq;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Services;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box derived unit tests for StorageUnitService.GetAvailableAsync.
/// 
/// Method under test:
/// GetAvailableAsync(Guid? facilityId = null, Guid? unitTypeId = null, decimal? maxPrice = null)
///
/// Purpose:
/// These tests cover the independent logical paths from the white-box graph:
/// 1. No filters.
/// 2. Facility filter rejects unit.
/// 3. Facility filter accepts unit.
/// 4. Unit type filter rejects unit.
/// 5. Unit type filter accepts unit.
/// 6. Max price filter rejects unit.
/// 7. Max price filter accepts unit.
///
/// The tests use Moq to isolate the service from the repository.
/// </summary>

public class StorageUnitServiceWhiteBoxTests
{
    private readonly Mock<IStorageUnitRepository> _storageUnitRepositoryMock;
    private readonly StorageUnitService _service;

    public StorageUnitServiceWhiteBoxTests()
    {
        _storageUnitRepositoryMock = new Mock<IStorageUnitRepository>();
        _service = new StorageUnitService(_storageUnitRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAvailableAsync_NoFilters_ReturnsAllAvailableUnits()
    {
        // Arrange
        var units = new List<StorageUnit>
        {
            CreateStorageUnit(monthlyPrice: 1000m),
            CreateStorageUnit(monthlyPrice: 1500m)
        };

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetAvailableUnitsAsync(null))
            .ReturnsAsync(units);

        // Act
        var result = await _service.GetAvailableAsync();

        // Assert
        Assert.Equal(2, result.Count);

        _storageUnitRepositoryMock.Verify(
            repository => repository.GetAvailableUnitsAsync(null),
            Times.Once);
    }

    [Fact]
    public async Task GetAvailableAsync_FacilityIdDoesNotMatch_ReturnsEmptyList()
    {
        // Arrange
        var requestedFacilityId = Guid.NewGuid();
        var differentFacilityId = Guid.NewGuid();

        var units = new List<StorageUnit>
        {
            CreateStorageUnit(facilityId: differentFacilityId)
        };

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetAvailableUnitsAsync(null))
            .ReturnsAsync(units);

        // Act
        var result = await _service.GetAvailableAsync(facilityId: requestedFacilityId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAvailableAsync_FacilityIdMatches_ReturnsMatchingUnit()
    {
        // Arrange
        var requestedFacilityId = Guid.NewGuid();

        var matchingUnit = CreateStorageUnit(facilityId: requestedFacilityId);
        var nonMatchingUnit = CreateStorageUnit(facilityId: Guid.NewGuid());

        var units = new List<StorageUnit>
        {
            matchingUnit,
            nonMatchingUnit
        };

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetAvailableUnitsAsync(null))
            .ReturnsAsync(units);

        // Act
        var result = await _service.GetAvailableAsync(facilityId: requestedFacilityId);

        // Assert
        var returnedUnit = Assert.Single(result);
        Assert.Equal(matchingUnit.Id, returnedUnit.Id);
    }

    [Fact]
    public async Task GetAvailableAsync_UnitTypeIdDoesNotMatch_ReturnsEmptyList()
    {
        // Arrange
        var requestedUnitTypeId = Guid.NewGuid();
        var differentUnitTypeId = Guid.NewGuid();

        var units = new List<StorageUnit>
        {
            CreateStorageUnit(unitTypeId: differentUnitTypeId)
        };

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetAvailableUnitsAsync(null))
            .ReturnsAsync(units);

        // Act
        var result = await _service.GetAvailableAsync(unitTypeId: requestedUnitTypeId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAvailableAsync_UnitTypeIdMatches_ReturnsMatchingUnit()
    {
        // Arrange
        var requestedUnitTypeId = Guid.NewGuid();

        var matchingUnit = CreateStorageUnit(unitTypeId: requestedUnitTypeId);
        var nonMatchingUnit = CreateStorageUnit(unitTypeId: Guid.NewGuid());

        var units = new List<StorageUnit>
        {
            matchingUnit,
            nonMatchingUnit
        };

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetAvailableUnitsAsync(null))
            .ReturnsAsync(units);

        // Act
        var result = await _service.GetAvailableAsync(unitTypeId: requestedUnitTypeId);

        // Assert
        var returnedUnit = Assert.Single(result);
        Assert.Equal(matchingUnit.Id, returnedUnit.Id);
    }

    [Fact]
    public async Task GetAvailableAsync_MonthlyPriceIsGreaterThanMaxPrice_ReturnsEmptyList()
    {
        // Arrange
        var units = new List<StorageUnit>
        {
            CreateStorageUnit(monthlyPrice: 1500m)
        };

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetAvailableUnitsAsync(null))
            .ReturnsAsync(units);

        // Act
        var result = await _service.GetAvailableAsync(maxPrice: 1000m);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(1000, 1000)]
    [InlineData(999, 1000)]
    public async Task GetAvailableAsync_MonthlyPriceIsLessThanOrEqualToMaxPrice_ReturnsMatchingUnit(
        decimal unitPrice,
        decimal maxPrice)
    {
        // Arrange
        var unit = CreateStorageUnit(monthlyPrice: unitPrice);

        var units = new List<StorageUnit>
        {
            unit
        };

        _storageUnitRepositoryMock
            .Setup(repository => repository.GetAvailableUnitsAsync(null))
            .ReturnsAsync(units);

        // Act
        var result = await _service.GetAvailableAsync(maxPrice: maxPrice);

        // Assert
        var returnedUnit = Assert.Single(result);
        Assert.Equal(unit.Id, returnedUnit.Id);
    }

    private static StorageUnit CreateStorageUnit(
        Guid? facilityId = null,
        Guid? unitTypeId = null,
        decimal monthlyPrice = 1000m)
    {
        return new StorageUnit
        {
            Id = Guid.NewGuid(),
            FacilityId = facilityId ?? Guid.NewGuid(),
            UnitTypeId = unitTypeId ?? Guid.NewGuid(),
            UnitNumber = "A-101",
            Floor = 1,
            MonthlyPrice = monthlyPrice,
            Status = StorageUnitStatus.Available,
            IsClimateControlled = false,
            IsDriveUp = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}