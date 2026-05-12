using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<User> GetByIdAsync(Guid id);
    Task<User> CreateAsync(string firstName, string lastName, string email, string phoneNumber, string password);
    Task<User> UpdateAsync(Guid id, string firstName, string lastName, string phoneNumber, bool isActive);
}
