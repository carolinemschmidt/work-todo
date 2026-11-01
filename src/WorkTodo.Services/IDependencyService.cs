using WorkTodo.Core.Entities;

namespace WorkTodo.Services;

public interface IDependencyService
{
    Task AddDependencyAsync(int todoItemId, int dependsOnTodoItemId, CancellationToken cancellationToken = default);

    Task RemoveDependencyAsync(int todoItemId, int dependsOnTodoItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TodoItem>> GetDependsOnAsync(int todoItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TodoItem>> GetDependentsAsync(int todoItemId, CancellationToken cancellationToken = default);

    Task AddFreeTextDependencyAsync(int todoItemId, string text, CancellationToken cancellationToken = default);

    Task ResolveFreeTextDependencyAsync(int todoItemId, int dependencyId, CancellationToken cancellationToken = default);

    Task ReopenFreeTextDependencyAsync(int todoItemId, int dependencyId, CancellationToken cancellationToken = default);

    Task RemoveFreeTextDependencyAsync(int todoItemId, int dependencyId, CancellationToken cancellationToken = default);
}
