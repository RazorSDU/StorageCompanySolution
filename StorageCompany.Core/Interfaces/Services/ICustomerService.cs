using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync();
    Task<Customer> GetByIdAsync(Guid id);
    Task<Customer> CreateAsync(string firstName, string lastName, string email, string phoneNumber, string password);
    Task<Customer> UpdateAsync(Guid id, string firstName, string lastName, string phoneNumber, bool isActive);
}
