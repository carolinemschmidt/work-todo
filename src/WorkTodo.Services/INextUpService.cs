using WorkTodo.Core.Entities;

namespace WorkTodo.Services;

public interface INextUpService
{
    Task<IReadOnlyList<TodoItem>> GetNextUpAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlySet<int>> GetBlockedIdsAsync(CancellationToken cancellationToken = default);
}
