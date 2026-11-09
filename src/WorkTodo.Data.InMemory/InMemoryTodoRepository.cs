using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.InMemory;

public class InMemoryTodoRepository(InMemoryDataStore store) : ITodoRepository
{
    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.Todos.FirstOrDefault(item => item.Id == id));
    }

    public Task<IReadOnlyList<TodoItem>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TodoItem>>(store.Todos.Where(item => ids.Contains(item.Id)).ToList());
    }

    public Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TodoItem>>(store.Todos.ToList());
    }

    public Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        store.Add(item);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        store.Update(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        store.Remove(item);
        return Task.CompletedTask;
    }
}
