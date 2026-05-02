using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Infrastructure.Data;

namespace StorageCompany.Infrastructure.Repositories;

public abstract class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    private readonly List<TEntity> _items;

    protected InMemoryRepository(List<TEntity> items)
    {
        _items = items;
    }

    public Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        lock (MockDatabase.SyncRoot)
        {
            return Task.FromResult<IReadOnlyList<TEntity>>(_items.OrderBy(x => x.CreatedAtUtc).ToList());
        }
    }

    public Task<TEntity?> GetByIdAsync(Guid id)
    {
        lock (MockDatabase.SyncRoot)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
        }
    }

    public Task AddAsync(TEntity entity)
    {
        lock (MockDatabase.SyncRoot)
        {
            _items.Add(entity);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity)
    {
        lock (MockDatabase.SyncRoot)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0)
                _items[index] = entity;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        lock (MockDatabase.SyncRoot)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null)
                _items.Remove(item);
        }

        return Task.CompletedTask;
    }
}
