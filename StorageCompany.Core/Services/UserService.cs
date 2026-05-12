using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users)
    {
        _users = users;
    }

    public Task<IReadOnlyList<User>> GetAllAsync() => _users.GetAllAsync();

    public async Task<User> GetByIdAsync(Guid id)
    {
        var customer = await _users.GetByIdAsync(id);
        return customer ?? throw new NotFoundException($"User '{id}' was not found.");
    }

    public async Task<User> CreateAsync(string firstName, string lastName, string email, string phoneNumber, string password)
    {
        Guard.AgainstBlank(firstName, nameof(firstName));
        Guard.AgainstBlank(lastName, nameof(lastName));
        Guard.AgainstBlank(email, nameof(email));
        Guard.AgainstBlank(password, nameof(password));

        var existing = await _users.GetByEmailAsync(email);
        if (existing is not null)
            throw new BusinessRuleException("A customer with this email already exists.");

        var customer = new User
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

        await _users.AddAsync(customer);
        return customer;
    }

    public async Task<User> UpdateAsync(Guid id, string firstName, string lastName, string phoneNumber, bool isActive)
    {
        var customer = await GetByIdAsync(id);

        Guard.AgainstBlank(firstName, nameof(firstName));
        Guard.AgainstBlank(lastName, nameof(lastName));

        customer.FirstName = firstName.Trim();
        customer.LastName = lastName.Trim();
        customer.PhoneNumber = phoneNumber.Trim();
        customer.IsActive = isActive;

        await _users.UpdateAsync(customer);
        return customer;
    }
}
