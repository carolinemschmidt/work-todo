using WorkTodo.Core.Entities;
using WorkTodo.Core.Rules;
using WorkTodo.Data.Repositories;
using WorkTodo.Services.Exceptions;
using WorkTodo.Services.Extensions;
using WorkTodo.Services.Queries;

namespace WorkTodo.Services;

public class DependencyService(ITodoRepository todos, ITodoDependencyRepository dependencies) : IDependencyService
{
    public async Task AddDependencyAsync(
        int todoItemId,
        int dependsOnTodoItemId,
        CancellationToken cancellationToken = default)
    {
        await todos.GetRequiredAsync(todoItemId, cancellationToken);
        await todos.GetRequiredAsync(dependsOnTodoItemId, cancellationToken);

        var existing = await dependencies.GetAllAsync(cancellationToken);
        if (existing.Any(dependency => Matches(dependency, todoItemId, dependsOnTodoItemId)))
        {
            return;
        }

        DependencyRules.EnsureCanAdd(todoItemId, dependsOnTodoItemId, existing);
        await dependencies.AddAsync(new TodoDependency(todoItemId, dependsOnTodoItemId), cancellationToken);
    }

    public async Task RemoveDependencyAsync(
        int todoItemId,
        int dependsOnTodoItemId,
        CancellationToken cancellationToken = default)
    {
        var existing = await dependencies.GetAllAsync(cancellationToken);
        var dependency = existing.FirstOrDefault(candidate => Matches(candidate, todoItemId, dependsOnTodoItemId));
        if (dependency is not null)
        {
            await dependencies.DeleteAsync(dependency, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<TodoItem>> GetDependsOnAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        var related = await dependencies.GetAllAsync(cancellationToken);
        var ids = related
            .Where(dependency => dependency.TodoItemId == todoItemId)
            .Select(dependency => dependency.DependsOnTodoItemId);

        return await GetOrderedItemsAsync(ids, cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetDependentsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        var related = await dependencies.GetAllAsync(cancellationToken);
        var ids = related
            .Where(dependency => dependency.DependsOnTodoItemId == todoItemId)
            .Select(dependency => dependency.TodoItemId);

        return await GetOrderedItemsAsync(ids, cancellationToken);
    }

    public async Task AddFreeTextDependencyAsync(
        int todoItemId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(todoItemId, cancellationToken);
        item.AddFreeTextDependency(text);
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task ResolveFreeTextDependencyAsync(
        int todoItemId,
        int dependencyId,
        CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(todoItemId, cancellationToken);
        FindFreeTextDependency(item, dependencyId).Resolve();
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task ReopenFreeTextDependencyAsync(
        int todoItemId,
        int dependencyId,
        CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(todoItemId, cancellationToken);
        FindFreeTextDependency(item, dependencyId).Reopen();
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task RemoveFreeTextDependencyAsync(
        int todoItemId,
        int dependencyId,
        CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(todoItemId, cancellationToken);
        item.RemoveFreeTextDependency(FindFreeTextDependency(item, dependencyId));
        await todos.UpdateAsync(item, cancellationToken);
    }

    private async Task<IReadOnlyList<TodoItem>> GetOrderedItemsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken)
    {
        var wanted = ids.ToHashSet();
        var items = await todos.GetAllAsync(cancellationToken);
        return TodoOrdering.ByUrgency(items.Where(item => wanted.Contains(item.Id))).ToList();
    }

    private static bool Matches(TodoDependency dependency, int todoItemId, int dependsOnTodoItemId)
    {
        return dependency.TodoItemId == todoItemId && dependency.DependsOnTodoItemId == dependsOnTodoItemId;
    }

    private static FreeTextDependency FindFreeTextDependency(TodoItem item, int dependencyId)
    {
        return item.FreeTextDependencies.FirstOrDefault(dependency => dependency.Id == dependencyId)
            ?? throw new NotFoundException(nameof(FreeTextDependency), dependencyId);
    }
}
