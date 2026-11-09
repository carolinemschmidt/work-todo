using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.InMemory;

public class InMemoryTagRepository(InMemoryDataStore store) : ITagRepository
{
    public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var ordered = store.Tags.OrderBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase).ToList();
        return Task.FromResult<IReadOnlyList<Tag>>(ordered);
    }

    public Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.Tags.FirstOrDefault(tag => tag.Id == id));
    }

    public Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.FindTag(name));
    }

    public Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        store.Add(tag);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        store.Remove(tag);
        return Task.CompletedTask;
    }
}
