using Reqnroll;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Services;
using StorageCompany.Specs.Fakes;
using Xunit;

namespace StorageCompany.Specs.StepDefinitions;

[Binding]
public class CreateAsyncSteps
{
    private readonly List<Customer> _customers = [];
    private readonly List<StorageUnit> _storageUnits = [];
    private readonly List<Reservation> _reservations = [];
    private readonly ReservationService _reservationService;

    private Guid _customerId = Guid.NewGuid();
    private Guid _storageUnitId = Guid.NewGuid();
    private Reservation? _reservation;
    private Exception? _exception;

    public CreateAsyncSteps()
    {
        _reservationService = new ReservationService(
            new FakeCustomerRepository(_customers),
            new FakeStorageUnitRepository(_storageUnits),
            new FakeReservationRepository(_reservations));
    }

    [Given("a customer that does not exist")]
    public void GivenCustomerDoesNotExist()
    {
        _customerId = Guid.NewGuid();
    }

    [Given("an inactive customer")]
    public void GivenInactiveCustomer()
    {
        var customer = new Customer { Id = Guid.NewGuid(), IsActive = false };
        _customers.Add(customer);
        _customerId = customer.Id;
    }

    [Given("an active customer")]
    public void GivenActiveCustomer()
    {
        var customer = new Customer { Id = Guid.NewGuid(), IsActive = true };
        _customers.Add(customer);
        _customerId = customer.Id;
    }

    [Given("a storage unit that does not exist")]
    public void GivenStorageUnitDoesNotExist()
    {
        _storageUnitId = Guid.NewGuid();
    }

    [Given("a storage unit that is not available")]
    public void GivenStorageUnitNotAvailable()
    {
        var storageUnit = new StorageUnit { Id = Guid.NewGuid(), MonthlyPrice = 500, Status = StorageUnitStatus.Rented };
        _storageUnits.Add(storageUnit);
        _storageUnitId = storageUnit.Id;
    }

    [Given("an available storage unit")]
    public void GivenAvailableStorageUnit()
    {
        var storageUnit = new StorageUnit { Id = Guid.NewGuid(), MonthlyPrice = 500, Status = StorageUnitStatus.Available };
        _storageUnits.Add(storageUnit);
        _storageUnitId = storageUnit.Id;
    }

    [When("the customer creates a reservation with a future move-in date")]
    public async Task WhenCreateWithFutureMoveInDate()
    {
        _exception = await Record.ExceptionAsync(async () =>
            _reservation = await _reservationService.CreateAsync(_customerId, _storageUnitId, DateTime.UtcNow.Date.AddDays(1)));
    }

    [When("the customer creates a reservation with a past move-in date")]
    public async Task WhenCreateWithPastMoveInDate()
    {
        _exception = await Record.ExceptionAsync(async () =>
            _reservation = await _reservationService.CreateAsync(_customerId, _storageUnitId, DateTime.UtcNow.Date.AddDays(-1)));
    }

    [Then("the reservation should not be created")]
    public void ThenReservationNotCreated() => Assert.NotNull(_exception);

    [Then("the reservation should be created successfully")]
    public void ThenReservationCreated() => Assert.NotNull(_reservation);

    [Then("the storage unit should be marked as reserved")]
    public void ThenStorageUnitIsReserved()
    {
        var storageUnit = _storageUnits.First(u => u.Id == _storageUnitId);
        Assert.Equal(StorageUnitStatus.Reserved, storageUnit.Status);
    }
}
