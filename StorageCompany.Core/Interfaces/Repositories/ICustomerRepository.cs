using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
}
