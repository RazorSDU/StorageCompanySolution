using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class AccessCodeService : IAccessCodeService
{
    private readonly IAccessCodeRepository _accessCodes;
    private readonly IRentalRepository _rentals;

    public AccessCodeService(IAccessCodeRepository accessCodes, IRentalRepository rentals)
    {
        _accessCodes = accessCodes;
        _rentals = rentals;
    }

    public async Task<AccessCode> GenerateForRentalAsync(Guid rentalId)
    {
        var rental = await _rentals.GetByIdAsync(rentalId)
            ?? throw new NotFoundException($"Rental '{rentalId}' was not found.");

        var existing = await _accessCodes.GetActiveByRentalIdAsync(rental.Id);
        if (existing is not null)
            return existing;

        var code = new AccessCode
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            Code = Random.Shared.Next(100000, 999999).ToString(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _accessCodes.AddAsync(code);
        return code;
    }

    public async Task<AccessCode> GetActiveByRentalIdAsync(Guid rentalId)
    {
        var code = await _accessCodes.GetActiveByRentalIdAsync(rentalId);
        return code ?? throw new NotFoundException($"No active access code was found for rental '{rentalId}'.");
    }

    public async Task DeactivateByRentalIdAsync(Guid rentalId)
    {
        var code = await _accessCodes.GetActiveByRentalIdAsync(rentalId);
        if (code is null)
            return;

        code.IsActive = false;
        code.ExpiresAtUtc = DateTime.UtcNow;
        await _accessCodes.UpdateAsync(code);
    }
}
