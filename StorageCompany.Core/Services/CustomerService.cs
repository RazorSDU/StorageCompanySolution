using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;

    public CustomerService(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public Task<IReadOnlyList<Customer>> GetAllAsync() => _customers.GetAllAsync();

    public async Task<Customer> GetByIdAsync(Guid id)
    {
        var customer = await _customers.GetByIdAsync(id);
        return customer ?? throw new NotFoundException($"Customer '{id}' was not found.");
    }

    public async Task<Customer> CreateAsync(string firstName, string lastName, string email, string phoneNumber, string password)
    {
        Guard.AgainstBlank(firstName, nameof(firstName));
        Guard.AgainstBlank(lastName, nameof(lastName));
        Guard.AgainstBlank(email, nameof(email));
        Guard.AgainstBlank(password, nameof(password));

        var existing = await _customers.GetByEmailAsync(email);
        if (existing is not null)
            throw new BusinessRuleException("A customer with this email already exists.");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            PasswordHash = $"MOCK_HASH::{password.Length}::{Guid.NewGuid():N}",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _customers.AddAsync(customer);
        return customer;
    }

    public async Task<Customer> UpdateAsync(Guid id, string firstName, string lastName, string phoneNumber, bool isActive)
    {
        var customer = await GetByIdAsync(id);

        Guard.AgainstBlank(firstName, nameof(firstName));
        Guard.AgainstBlank(lastName, nameof(lastName));

        customer.FirstName = firstName.Trim();
        customer.LastName = lastName.Trim();
        customer.PhoneNumber = phoneNumber.Trim();
        customer.IsActive = isActive;

        await _customers.UpdateAsync(customer);
        return customer;
    }
}
