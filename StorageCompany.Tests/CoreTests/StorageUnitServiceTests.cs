using StorageCompany.Core.Enums;
using StorageCompany.Core.Services;
using StorageCompany.Infrastructure.Data;
using StorageCompany.Infrastructure.Repositories;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

/// <summary>
/// White-box tests for StorageUnitService.GetAvailableAsync().
/// One test per DD-path from the white-box testing diagram (CC = 6).
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

    // Path 1: units is empty, so the loop is never entered.
    [Fact]
    public async Task GetAvailableAsync_WhenNoUnitsAreAvailable_ReturnsEmptyList()
    {
        lock (MockDatabase.SyncRoot)
            foreach (var u in MockDatabase.StorageUnits)
                u.Status = StorageUnitStatus.Rented;

        var result = await _sut.GetAvailableAsync();

        Assert.Empty(result);
    }

    // Path 2: A unit is rejected because the facility filter does not match.
    [Fact]
    public async Task GetAvailableAsync_WhenFacilityFilterDoesNotMatchAnyAvailableUnit_ReturnsEmptyList()
    {
        // FacilityOdense has no Available units (UnitOdenseMedium is Maintenance)
        var result = await _sut.GetAvailableAsync(facilityId: MockDatabase.Ids.FacilityOdense);

        Assert.Empty(result);
    }

    // Path 3: A unit passes the facility check but is rejected because the unit type filter does not match.
    [Fact]
    public async Task GetAvailableAsync_WhenUnitTypeFilterDoesNotMatchUnitInFacility_ReturnsEmptyList()
    {
        // FacilityAarhus has Small + Business — filtering for Large finds nothing
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeLarge);

        Assert.Empty(result);
    }

    // Path 4: A unit passes facility and unit type checks but is rejected because the price is too high.
    [Fact]
    public async Task GetAvailableAsync_WhenMaxPriceIsLowerThanAllUnitPrices_ReturnsEmptyList()
    {
        // Cheapest available unit is AarhusSmall at 299 kr — 1 kr is below all
        var result = await _sut.GetAvailableAsync(maxPrice: 1m);

        Assert.Empty(result);
    }

    // Path 5: A unit passes all active filters and is added to the result list.
    [Fact]
    public async Task GetAvailableAsync_WhenUnitPassesAllFilters_ReturnsThatUnit()
    {
        // AarhusSmall: Aarhus facility + Small type + 299 kr — passes all three filters
        var result = await _sut.GetAvailableAsync(
            facilityId: MockDatabase.Ids.FacilityAarhus,
            unitTypeId: MockDatabase.Ids.UnitTypeSmall,
            maxPrice: 500m);

        Assert.Single(result);
        Assert.Equal(MockDatabase.Ids.UnitAarhusSmall, result[0].Id);
    }

    // Path 6: More than one unit exists, so the loop repeats and processes another unit.
    [Fact]
    public async Task GetAvailableAsync_WhenMultipleUnitsAreAvailable_ReturnsAllOfThem()
    {
        // No filters — all 4 seeded Available units are returned
        var result = await _sut.GetAvailableAsync();

        Assert.Equal(4, result.Count);
    }
}
