using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;

namespace StorageCompany.Specs.Fakes;

public class FakeCustomerRepository(List<User> customers) : IUserRepository
{
    private readonly List<User> _customers = customers;

    public Task<User?> GetByIdAsync(Guid id) =>
        Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(User entity) { _customers.Add(entity); return Task.CompletedTask; }

    public Task UpdateAsync(User entity)
    {
        var index = _customers.FindIndex(c => c.Id == entity.Id);
        if (index >= 0) _customers[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id) { _customers.RemoveAll(c => c.Id == id); return Task.CompletedTask; }

    public Task<IReadOnlyList<User>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<User>>([.. _customers]);

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_customers.FirstOrDefault(c =>
            c.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task<User?> AddUser(User user)
    {
        _customers.Add(user);
        return Task.FromResult<User?>(user);
    }
}
