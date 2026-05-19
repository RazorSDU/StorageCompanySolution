using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Services;
using StorageCompany.Infrastructure.Data;
using StorageCompany.Infrastructure.Repositories;
using Xunit;

namespace StorageCompany.Tests.CoreTests;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateAsync_WithAvailableUnit_ReservesTheUnit()
    {
        var service = new ReservationService(
            new UserRepository(),
            new StorageUnitRepository(),
            new ReservationRepository());

        var unit = MockDatabase.StorageUnits.First(x => x.Id == MockDatabase.Ids.UnitAarhusSmall);
        unit.Status = StorageUnitStatus.Available;

        var reservation = await service.CreateAsync(
            MockDatabase.Ids.UserPeter,
            MockDatabase.Ids.UnitAarhusSmall,
            DateTime.UtcNow.Date.AddDays(1));

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(StorageUnitStatus.Reserved, unit.Status);
    }

    [Fact]
    public async Task CreateAsync_WithRentedUnit_ThrowsBusinessRuleException()
    {
        var service = new ReservationService(
            new UserRepository(),
            new StorageUnitRepository(),
            new ReservationRepository());

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(
            MockDatabase.Ids.UserPeter,
            MockDatabase.Ids.UnitCphMedium,
            DateTime.UtcNow.Date.AddDays(1)));
    }
}
