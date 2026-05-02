using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class CustomerRepository : InMemoryRepository<Customer>, ICustomerRepository
{
    public CustomerRepository() : base(MockDatabase.Customers)
    {
    }

    public Task<Customer?> GetByEmailAsync(string email)
    {
        lock (MockDatabase.SyncRoot)
        {
            return Task.FromResult(MockDatabase.Customers.FirstOrDefault(x => x.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)));
        }
    }
}
