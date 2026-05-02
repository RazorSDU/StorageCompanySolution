using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class SupportRequestService : ISupportRequestService
{
    private readonly ISupportRequestRepository _supportRequests;
    private readonly ICustomerRepository _customers;
    private readonly IRentalRepository _rentals;

    public SupportRequestService(
        ISupportRequestRepository supportRequests,
        ICustomerRepository customers,
        IRentalRepository rentals)
    {
        _supportRequests = supportRequests;
        _customers = customers;
        _rentals = rentals;
    }

    public async Task<SupportRequest> CreateAsync(Guid customerId, Guid? rentalId, string subject, string message)
    {
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstBlank(subject, nameof(subject));
        Guard.AgainstBlank(message, nameof(message));

        var customer = await _customers.GetByIdAsync(customerId)
            ?? throw new NotFoundException($"Customer '{customerId}' was not found.");

        if (!customer.IsActive)
            throw new BusinessRuleException("Inactive customers cannot create support requests.");

        if (rentalId.HasValue)
        {
            var rental = await _rentals.GetByIdAsync(rentalId.Value)
                ?? throw new NotFoundException($"Rental '{rentalId}' was not found.");

            if (rental.CustomerId != customerId)
                throw new BusinessRuleException("The selected rental does not belong to this customer.");
        }

        var request = new SupportRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            RentalId = rentalId,
            Subject = subject.Trim(),
            Message = message.Trim(),
            Status = SupportRequestStatus.Open,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _supportRequests.AddAsync(request);
        return request;
    }

    public async Task<SupportRequest> GetByIdAsync(Guid id)
    {
        var request = await _supportRequests.GetByIdAsync(id);
        return request ?? throw new NotFoundException($"Support request '{id}' was not found.");
    }

    public Task<IReadOnlyList<SupportRequest>> GetAllAsync()
    {
        return _supportRequests.GetAllAsync();
    }

    public Task<IReadOnlyList<SupportRequest>> GetByCustomerIdAsync(Guid customerId)
    {
        return _supportRequests.GetByCustomerIdAsync(customerId);
    }

    public async Task<SupportRequest> UpdateStatusAsync(Guid id, SupportRequestStatus status)
    {
        var request = await GetByIdAsync(id);
        request.Status = status;
        await _supportRequests.UpdateAsync(request);
        return request;
    }
}
