using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(Guid id);
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(Guid id);
}
