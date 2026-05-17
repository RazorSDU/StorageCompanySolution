using Reqnroll;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Services;
using StorageCompany.Specs.Fakes;
using Xunit;

namespace StorageCompany.Specs.StepDefinitions;

[Binding]
public class GetAvailableAsyncSteps
{
    private static readonly Guid facilityAId = Guid.NewGuid();
    private static readonly Guid facilityBId = Guid.NewGuid();
    private static readonly Guid facilityCId = Guid.NewGuid();
    private static readonly Guid unitTypeSmallId = Guid.NewGuid();
    private static readonly Guid unitTypeMediumId = Guid.NewGuid();
    private static readonly Guid unitTypeLargeId = Guid.NewGuid();

    private readonly List<StorageUnit> _storageUnits = [];
    private readonly StorageUnitService _storageUnitService;
    private IReadOnlyList<StorageUnit> searchResults = [];

    public GetAvailableAsyncSteps()
    {
        _storageUnitService = new StorageUnitService(new FakeStorageUnitRepository(_storageUnits));
    }

    private static Guid ResolveStorageUnitType(string name) => name switch
    {
        "Small" => unitTypeSmallId,
        "Medium" => unitTypeMediumId,
        "Large" => unitTypeLargeId,
        _ => throw new ArgumentException($"Unknown unit type: {name}")
    };

    private static Guid ResolveFacility(string name) => name switch
    {
        "Facility A" => facilityAId,
        "Facility B" => facilityBId,
        "Facility C" => facilityCId,
        _ => throw new ArgumentException($"Unknown facility: {name}")
    };

    [Given("the following storage units exist:")]
    public void GivenTheFollowingStorageUnitExists(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            _storageUnits.Add(new StorageUnit
            {
                Id = Guid.NewGuid(),
                FacilityId = ResolveFacility(row["Facility"]),
                UnitTypeId = ResolveStorageUnitType(row["UnitType"]),
                UnitNumber = row["UnitNumber"],
                MonthlyPrice = decimal.Parse(row["MonthlyPrice"]),
                Status = StorageUnitStatus.Available
            });
        }
    }

    [When("I search for available storage units with no filters")]
    public async Task WhenISearchWithNoFilters()
    {
        searchResults = await _storageUnitService.GetAvailableAsync();
    }

    [When("I search for available storage units with a max price of {int}")]
    public async Task WhenISearchWithMaxPrice(int maxPrice)
    {
        searchResults = await _storageUnitService.GetAvailableAsync(maxPrice: maxPrice);
    }

    [When("I search for available storage units with unit type {string}")]
    public async Task WhenISearchWithStorageUnitType(string unitType)
    {
        searchResults = await _storageUnitService.GetAvailableAsync(unitTypeId: ResolveStorageUnitType(unitType));
    }

    [When("I search for available storage units from facility {string}")]
    public async Task WhenISearchWithFacility(string facility)
    {
        searchResults = await _storageUnitService.GetAvailableAsync(facilityId: ResolveFacility(facility));
    }

    [When("I search for available storage units from facility {string} with unit type {string} and max price of {int}")]
    public async Task WhenISearchWithAllFilters(string facility, string unitType, int maxPrice)
    {
        searchResults = await _storageUnitService.GetAvailableAsync(
            facilityId: ResolveFacility(facility),
            unitTypeId: ResolveStorageUnitType(unitType),
            maxPrice: maxPrice);
    }

    [Then("I should receive a non-empty list of storage units")]
    public void ThenResultIsNonEmpty()
    {
        Assert.NotEmpty(searchResults);
    }

    [Then("I should receive an empty list of storage units")]
    public void ThenResultIsEmpty()
    {
        Assert.Empty(searchResults);
    }
}
