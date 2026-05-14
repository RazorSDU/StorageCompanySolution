using StorageCompany.Core.Enums;
using StorageCompany.Core.Services;
using StorageCompany.Infrastructure.Data;
using StorageCompany.Infrastructure.Repositories;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box tests for StorageUnitService.GetAvailableAsync() — CC = 6.
///
/// Seeded Available units:
///   UnitCphSmall       CPH,    Small,    349 kr
///   UnitCphLarge       CPH,    Large,   1199 kr
///   UnitAarhusSmall    Aarhus, Small,    299 kr
///   UnitAarhusBusiness Aarhus, Business 1699 kr
/// </summary>
[Collection("MockDatabase")]
public class StorageUnitServiceTests
{
    private readonly StorageUnitService _sut;

    public StorageUnitServiceTests()
    {
        _sut = new StorageUnitService(new StorageUnitRepository());
        ResetUnits();
    }

    private static void ResetUnits()
    {
        lock (MockDatabase.SyncRoot)
        {
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitCphSmall).Status       = StorageUnitStatus.Available;
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitCphMedium).Status      = StorageUnitStatus.Rented;
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitCphLarge).Status       = StorageUnitStatus.Available;
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusSmall).Status    = StorageUnitStatus.Available;
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitAarhusBusiness).Status = StorageUnitStatus.Available;
            MockDatabase.StorageUnits.First(u => u.Id == MockDatabase.Ids.UnitOdenseMedium).Status   = StorageUnitStatus.Maintenance;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // DD-Path tests (Paths 1–6)
    // ══════════════════════════════════════════════════════════════════

    // Path 1: units is empty, so the loop is never entered.
    [Fact]
    public async Task GetAvailableAsync_WhenNoUnitsAreAvailable_ReturnsEmptyList()
    {
        // Arrange
        lock (MockDatabase.SyncRoot)
            foreach (var u in MockDatabase.StorageUnits)
                u.Status = StorageUnitStatus.Rented;

        // Act
        var result = await _sut.GetAvailableAsync();

        // Assert
        Assert.Empty(result);
    }

    // Path 2: A unit is rejected because the facility filter does not match.
    [Fact]
    public async Task GetAvailableAsync_WhenFacilityFilterDoesNotMatch_ReturnsEmptyList()
    {
        // Arrange
        // FacilityOdense has no Available units

        // Act
        var result = await _sut.GetAvailableAsync(facilityId: MockDatabase.Ids.FacilityOdense);

        // Assert
        Assert.Empty(result);
    }

    // Path 3: A unit passes the facility check but is rejected because the unit type filter does not match.
    [Fact]
    public async Task GetAvailableAsync_WhenUnitTypeFilterDoesNotMatch_ReturnsEmptyList()
    {
        // Arrange
        // Aarhus has Small + Business — filtering for Large finds nothing

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeLarge);

        // Assert
        Assert.Empty(result);
    }

    // Path 4: A unit passes facility and unit type checks but is rejected because the price is too high.
    [Fact]
    public async Task GetAvailableAsync_WhenMaxPriceIsLowerThanAllUnitPrices_ReturnsEmptyList()
    {
        // Arrange
        var maxPrice = 1m;

        // Act
        var result = await _sut.GetAvailableAsync(maxPrice: maxPrice);

        // Assert
        Assert.Empty(result);
    }

    // Path 5: A unit passes all active filters and is added to the result list.
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPassesAllFilters_ReturnsThatUnit()
    {
        // Arrange
        // AarhusSmall: Aarhus, Small, 299 kr — passes all three filters

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeSmall,
            maxPrice: 500m);

        // Assert
        Assert.Single(result);
        Assert.Equal(MockDatabase.Ids.UnitAarhusSmall, result[0].Id);
    }

    // Path 6: More than one unit exists, so the loop repeats and processes another unit.
    [Fact]
    public async Task GetAvailableAsync_WhenMultipleUnitsAreAvailable_ReturnsAllOfThem()
    {
        // Arrange
        // No filters — all 4 seeded Available units are returned

        // Act
        var result = await _sut.GetAvailableAsync();

        // Assert
        Assert.Equal(4, result.Count);
    }

    // ══════════════════════════════════════════════════════════════════
    // Coverage — filter not selected (HasValue = false → branch skipped)
    // ══════════════════════════════════════════════════════════════════

    // Unit exists, but facility filter is not selected
    [Fact]
    public async Task GetAvailableAsync_WhenFacilityFilterIsNull_DoesNotFilterByFacility()
    {
        // Arrange
        // No facilityId → facility check is skipped entirely
        // Filtering only by Small type and price ≤ 500 → returns CphSmall (349) and AarhusSmall (299)

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: null,
            unitTypeId: MockDatabase.Ids.UnitTypeSmall,
            maxPrice: 500m);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == MockDatabase.Ids.UnitCphSmall);
        Assert.Contains(result, u => u.Id == MockDatabase.Ids.UnitAarhusSmall);
    }

    // Unit exists, but unit type filter is not selected
    [Fact]
    public async Task GetAvailableAsync_WhenUnitTypeFilterIsNull_DoesNotFilterByUnitType()
    {
        // Arrange
        // No unitTypeId → type check is skipped entirely
        // Filtering only by Aarhus facility and price ≤ 500 → AarhusSmall (299) passes, AarhusBusiness (1699) is rejected by price

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: null,
            maxPrice: 500m);

        // Assert
        Assert.Single(result);
        Assert.Equal(MockDatabase.Ids.UnitAarhusSmall, result[0].Id);
    }

    // Unit exists, but max price filter is not selected
    [Fact]
    public async Task GetAvailableAsync_WhenMaxPriceFilterIsNull_DoesNotFilterByPrice()
    {
        // Arrange
        // No maxPrice → price check is skipped entirely
        // Filtering only by Aarhus facility and Small type → AarhusSmall regardless of price

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeSmall,
            maxPrice: null);

        // Assert
        Assert.Single(result);
        Assert.Equal(MockDatabase.Ids.UnitAarhusSmall, result[0].Id);
    }

    // ══════════════════════════════════════════════════════════════════
    // Boundary value — maxPrice boundary (operator is strict >)
    // MonthlyPrice > maxPrice.Value → rejected; MonthlyPrice == maxPrice → accepted
    // Using AarhusSmall with MonthlyPrice = 299 kr
    // ══════════════════════════════════════════════════════════════════

    // Unit price is lower than max price → unit is returned
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPriceIsLowerThanMaxPrice_ReturnsUnit()
    {
        // Arrange
        // 299 < 300

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeSmall,
            maxPrice: 300m);

        // Assert
        Assert.Single(result);
        Assert.Equal(MockDatabase.Ids.UnitAarhusSmall, result[0].Id);
    }

    // Unit price is equal to max price → unit is returned (boundary: > is strict, so equal passes)
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPriceEqualsMaxPrice_ReturnsUnit()
    {
        // Arrange
        // 299 == 299 → 299 > 299 is false → unit is NOT skipped

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeSmall,
            maxPrice: 299m);

        // Assert
        Assert.Single(result);
        Assert.Equal(MockDatabase.Ids.UnitAarhusSmall, result[0].Id);
    }

    // Unit price is higher than max price → unit is not returned
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPriceIsHigherThanMaxPrice_DoesNotReturnUnit()
    {
        // Arrange
        // 299 > 298 → unit is skipped

        // Act
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeSmall,
            maxPrice: 298m);

        // Assert
        Assert.Empty(result);
    }
}
