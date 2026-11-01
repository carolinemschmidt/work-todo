using WorkTodo.Core.Entities;

namespace WorkTodo.Services;

public interface INextUpService
{
    Task<IReadOnlyList<TodoItem>> GetNextUpAsync(CancellationToken cancellationToken = default);
}
