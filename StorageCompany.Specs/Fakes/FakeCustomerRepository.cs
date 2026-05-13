using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;

namespace StorageCompany.Specs.Fakes;

public class FakeCustomerRepository(List<Customer> customers) : ICustomerRepository
{
    private readonly List<Customer> _customers = customers;

    public Task<Customer?> GetByIdAsync(Guid id) =>
        Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(Customer entity) { _customers.Add(entity); return Task.CompletedTask; }

    public Task UpdateAsync(Customer entity)
    {
        var index = _customers.FindIndex(c => c.Id == entity.Id);
        if (index >= 0) _customers[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id) { _customers.RemoveAll(c => c.Id == id); return Task.CompletedTask; }

    public Task<IReadOnlyList<Customer>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Customer>>([.. _customers]);

    public Task<Customer?> GetByEmailAsync(string email) =>
        Task.FromResult(_customers.FirstOrDefault(c =>
            c.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)));
}
