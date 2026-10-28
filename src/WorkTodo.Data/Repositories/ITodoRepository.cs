using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Repositories;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(TodoItem item, CancellationToken cancellationToken = default);

    Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default);

    Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default);
}
