using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public class UserRepository : InMemoryRepository<User>, IUserRepository
{
    public UserRepository() : base(MockDatabase.Users)
    {
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        lock (MockDatabase.SyncRoot)
        {
            return Task.FromResult(MockDatabase.Users.FirstOrDefault(x => x.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task<User?> AddUser(User user)
    {
        lock (MockDatabase.SyncRoot)
        {
            MockDatabase.Users.Add(user);
            return Task.FromResult<User?>(user);
        }
    }
}
