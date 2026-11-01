using Microsoft.EntityFrameworkCore;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Repositories;

public class TodoDependencyRepository(AppDbContext context) : ITodoDependencyRepository
{
    public async Task<IReadOnlyList<TodoDependency>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TodoDependencies.ToListAsync(cancellationToken);
    }

    public async Task<TodoDependency?> GetAsync(
        int todoItemId,
        int dependsOnTodoItemId,
        CancellationToken cancellationToken = default)
    {
        return await context.TodoDependencies.FirstOrDefaultAsync(
            dependency => dependency.TodoItemId == todoItemId
                && dependency.DependsOnTodoItemId == dependsOnTodoItemId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetDependsOnIdsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        return await context.TodoDependencies
            .Where(dependency => dependency.TodoItemId == todoItemId)
            .Select(dependency => dependency.DependsOnTodoItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetDependentIdsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        return await context.TodoDependencies
            .Where(dependency => dependency.DependsOnTodoItemId == todoItemId)
            .Select(dependency => dependency.TodoItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        context.TodoDependencies.Add(dependency);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        context.TodoDependencies.Remove(dependency);
        await context.SaveChangesAsync(cancellationToken);
    }
}
