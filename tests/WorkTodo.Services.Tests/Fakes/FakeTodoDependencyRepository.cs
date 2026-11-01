using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Services.Tests.Fakes;

internal sealed class FakeTodoDependencyRepository : ITodoDependencyRepository
{
    private readonly List<TodoDependency> _dependencies = [];

    public IReadOnlyList<TodoDependency> Dependencies => _dependencies;

    public void Seed(params TodoDependency[] dependencies) => _dependencies.AddRange(dependencies);

    public Task<IReadOnlyList<TodoDependency>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TodoDependency>>(_dependencies.ToList());
    }

    public Task<TodoDependency?> GetAsync(
        int todoItemId,
        int dependsOnTodoItemId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dependencies.FirstOrDefault(
            dependency => dependency.TodoItemId == todoItemId
                && dependency.DependsOnTodoItemId == dependsOnTodoItemId));
    }

    public Task<IReadOnlyList<int>> GetDependsOnIdsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<int>>(_dependencies
            .Where(dependency => dependency.TodoItemId == todoItemId)
            .Select(dependency => dependency.DependsOnTodoItemId)
            .ToList());
    }

    public Task<IReadOnlyList<int>> GetDependentIdsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<int>>(_dependencies
            .Where(dependency => dependency.DependsOnTodoItemId == todoItemId)
            .Select(dependency => dependency.TodoItemId)
            .ToList());
    }

    public Task AddAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        _dependencies.Add(dependency);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        _dependencies.Remove(dependency);
        return Task.CompletedTask;
    }
}
