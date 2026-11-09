using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.InMemory;

public class InMemoryTodoDependencyRepository(InMemoryDataStore store) : ITodoDependencyRepository
{
    public Task<IReadOnlyList<TodoDependency>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TodoDependency>>(store.Dependencies.ToList());
    }

    public Task<TodoDependency?> GetAsync(
        int todoItemId,
        int dependsOnTodoItemId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.Dependencies.FirstOrDefault(
            dependency => dependency.TodoItemId == todoItemId
                && dependency.DependsOnTodoItemId == dependsOnTodoItemId));
    }

    public Task<IReadOnlyList<int>> GetDependsOnIdsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        var ids = store.Dependencies
            .Where(dependency => dependency.TodoItemId == todoItemId)
            .Select(dependency => dependency.DependsOnTodoItemId)
            .ToList();
        return Task.FromResult<IReadOnlyList<int>>(ids);
    }

    public Task<IReadOnlyList<int>> GetDependentIdsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        var ids = store.Dependencies
            .Where(dependency => dependency.DependsOnTodoItemId == todoItemId)
            .Select(dependency => dependency.TodoItemId)
            .ToList();
        return Task.FromResult<IReadOnlyList<int>>(ids);
    }

    public Task AddAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        store.Add(dependency);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        store.Dependencies.RemoveAll(existing => existing.TodoItemId == dependency.TodoItemId
            && existing.DependsOnTodoItemId == dependency.DependsOnTodoItemId);
        return Task.CompletedTask;
    }
}
