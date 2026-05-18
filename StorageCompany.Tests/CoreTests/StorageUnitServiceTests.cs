using Moq;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Services;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box tests for StorageUnitService.GetAvailableAsync() — CC = 6.
///
/// Paths 1–6  : DD-path tests
/// Paths 7–9  : Coverage — filter not selected (HasValue = false)
/// Paths 10–12: Boundary value tests on MonthlyPrice
/// </summary>
public class StorageUnitServiceTests
{
    private readonly Mock<IStorageUnitRepository> _repositoryMock;
    private readonly StorageUnitService           _sut;

    // Fixed IDs used across tests — defined once, no magic GUIDs inline
    private static readonly Guid FacilityA  = Guid.NewGuid();
    private static readonly Guid FacilityB  = Guid.NewGuid();
    private static readonly Guid TypeSmall  = Guid.NewGuid();
    private static readonly Guid TypeLarge  = Guid.NewGuid();

    public StorageUnitServiceTests()
    {
        _repositoryMock = new Mock<IStorageUnitRepository>();
        _sut = new StorageUnitService(_repositoryMock.Object);
    }

    // ── Test data builder ─────────────────────────────────────────────
    private static StorageUnit MakeUnit(Guid facilityId, Guid unitTypeId, decimal monthlyPrice) =>
        new() { FacilityId = facilityId, UnitTypeId = unitTypeId, MonthlyPrice = monthlyPrice };

    private void SetupUnits(params StorageUnit[] units) =>
        _repositoryMock
            .Setup(r => r.GetAvailableUnitsAsync(It.IsAny<Guid?>()))
            .ReturnsAsync((IReadOnlyList<StorageUnit>)units.ToList());

    // ══════════════════════════════════════════════════════════════════
    // DD-path tests (Paths 1–6)
    // ══════════════════════════════════════════════════════════════════

    // Path 1: units is empty, so the loop is never entered.
    [Fact]
    public async Task GetAvailableAsync_WhenNoUnitsAreAvailable_ReturnsEmptyList()
    {
        // Arrange
        SetupUnits();

        // Act
        var result = await _sut.GetAvailableAsync();

        // Assert
        Assert.Empty(result);
    }

    // Path 2: A unit is rejected because the facility filter does not match.
    [Fact]
    public async Task GetAvailableAsync_WhenFacilityFilterDoesNotMatch_ReturnsEmptyList()
    {
        // Arrange — unit belongs to FacilityB, filter asks for FacilityA
        SetupUnits(MakeUnit(FacilityB, TypeSmall, 299m));

        // Act
        var result = await _sut.GetAvailableAsync(facilityId: FacilityA);

        // Assert
        Assert.Empty(result);
    }

    // Path 3: A unit passes the facility check but is rejected because the unit type filter does not match.
    [Fact]
    public async Task GetAvailableAsync_WhenUnitTypeFilterDoesNotMatch_ReturnsEmptyList()
    {
        // Arrange — unit is TypeLarge, filter asks for TypeSmall
        SetupUnits(MakeUnit(FacilityA, TypeLarge, 299m));

        // Act
        var result = await _sut.GetAvailableAsync(facilityId: FacilityA, unitTypeId: TypeSmall);

        // Assert
        Assert.Empty(result);
    }

    // Path 4: A unit passes facility and unit type checks but is rejected because the price is too high.
    [Fact]
    public async Task GetAvailableAsync_WhenMaxPriceIsLowerThanUnitPrice_ReturnsEmptyList()
    {
        // Arrange — unit costs 500, filter allows max 299
        SetupUnits(MakeUnit(FacilityA, TypeSmall, 500m));

        // Act
        var result = await _sut.GetAvailableAsync(facilityId: FacilityA, unitTypeId: TypeSmall, maxPrice: 299m);

        // Assert
        Assert.Empty(result);
    }

    // Path 5: A unit passes all active filters and is added to the result list.
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPassesAllFilters_ReturnsThatUnit()
    {
        // Arrange
        var unit = MakeUnit(FacilityA, TypeSmall, 299m);
        SetupUnits(unit);

        // Act
        var result = await _sut.GetAvailableAsync(facilityId: FacilityA, unitTypeId: TypeSmall, maxPrice: 500m);

        // Assert
        Assert.Single(result);
        Assert.Same(unit, result[0]);
    }

    // Path 6: More than one unit exists, so the loop repeats and processes another unit.
    [Fact]
    public async Task GetAvailableAsync_WhenMultipleUnitsAreAvailable_ReturnsAllOfThem()
    {
        // Arrange — three units, no filters
        SetupUnits(
            MakeUnit(FacilityA, TypeSmall, 299m),
            MakeUnit(FacilityA, TypeLarge, 599m),
            MakeUnit(FacilityB, TypeSmall, 349m));

        // Act
        var result = await _sut.GetAvailableAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    // ══════════════════════════════════════════════════════════════════
    // Coverage — filter not selected (HasValue = false → branch skipped)
    // ══════════════════════════════════════════════════════════════════

    // Unit exists, but facility filter is not selected
    [Fact]
    public async Task GetAvailableAsync_WhenFacilityFilterIsNull_DoesNotFilterByFacility()
    {
        // Arrange — unit belongs to FacilityB; no facilityId filter means the check is skipped
        var unit = MakeUnit(FacilityB, TypeSmall, 299m);
        SetupUnits(unit);

        // Act — only unitTypeId and maxPrice are active
        var result = await _sut.GetAvailableAsync(unitTypeId: TypeSmall, maxPrice: 500m);

        // Assert
        Assert.Single(result);
        Assert.Same(unit, result[0]);
    }

    // Unit exists, but unit type filter is not selected
    [Fact]
    public async Task GetAvailableAsync_WhenUnitTypeFilterIsNull_DoesNotFilterByUnitType()
    {
        // Arrange — unit is TypeLarge; no unitTypeId filter means the check is skipped
        var unit = MakeUnit(FacilityA, TypeLarge, 299m);
        SetupUnits(unit);

        // Act — only facilityId and maxPrice are active
        var result = await _sut.GetAvailableAsync(facilityId: FacilityA, maxPrice: 500m);

        // Assert
        Assert.Single(result);
        Assert.Same(unit, result[0]);
    }

    // Unit exists, but max price filter is not selected
    [Fact]
    public async Task GetAvailableAsync_WhenMaxPriceFilterIsNull_DoesNotFilterByPrice()
    {
        // Arrange — unit costs 9999; no maxPrice filter means the check is skipped
        var unit = MakeUnit(FacilityA, TypeSmall, 9999m);
        SetupUnits(unit);

        // Act — only facilityId and unitTypeId are active
        var result = await _sut.GetAvailableAsync(facilityId: FacilityA, unitTypeId: TypeSmall);

        // Assert
        Assert.Single(result);
        Assert.Same(unit, result[0]);
    }

    // ══════════════════════════════════════════════════════════════════
    // Boundary value — MonthlyPrice vs maxPrice (operator is strict >)
    // 299 > 299 = false → passes; 300 > 299 = true → rejected
    // ══════════════════════════════════════════════════════════════════

    // Unit price is lower than max price → unit is returned
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPriceIsLowerThanMaxPrice_ReturnsUnit()
    {
        // Arrange — 298 < 299
        var unit = MakeUnit(FacilityA, TypeSmall, 298m);
        SetupUnits(unit);

        // Act
        var result = await _sut.GetAvailableAsync(maxPrice: 299m);

        // Assert
        Assert.Single(result);
        Assert.Same(unit, result[0]);
    }

    // Unit price is equal to max price → unit is returned (boundary: > is strict, so equal passes)
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPriceEqualsMaxPrice_ReturnsUnit()
    {
        // Arrange — 299 == 299 → 299 > 299 is false → not rejected
        var unit = MakeUnit(FacilityA, TypeSmall, 299m);
        SetupUnits(unit);

        // Act
        var result = await _sut.GetAvailableAsync(maxPrice: 299m);

        // Assert
        Assert.Single(result);
        Assert.Same(unit, result[0]);
    }

    // Unit price is higher than max price → unit is not returned
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPriceIsHigherThanMaxPrice_DoesNotReturnUnit()
    {
        // Arrange — 300 > 299 → rejected
        SetupUnits(MakeUnit(FacilityA, TypeSmall, 300m));

        // Act
        var result = await _sut.GetAvailableAsync(maxPrice: 299m);

        // Assert
        Assert.Empty(result);
    }
}
