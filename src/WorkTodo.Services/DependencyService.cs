using WorkTodo.Core.Entities;
using WorkTodo.Core.Rules;
using WorkTodo.Data.Repositories;
using WorkTodo.Services.Constants;
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
        var item = await todos.GetRequiredAsync(todoItemId, cancellationToken);
        var dependsOn = await todos.GetRequiredAsync(dependsOnTodoItemId, cancellationToken);

        if (await dependencies.GetAsync(todoItemId, dependsOnTodoItemId, cancellationToken) is not null)
        {
            return;
        }

        await EnsureNoCycleAsync(item, dependsOn, cancellationToken);
        await dependencies.AddAsync(new TodoDependency(todoItemId, dependsOnTodoItemId), cancellationToken);
    }

    public async Task RemoveDependencyAsync(
        int todoItemId,
        int dependsOnTodoItemId,
        CancellationToken cancellationToken = default)
    {
        var dependency = await dependencies.GetAsync(todoItemId, dependsOnTodoItemId, cancellationToken);
        if (dependency is not null)
        {
            await dependencies.DeleteAsync(dependency, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<TodoItem>> GetDependsOnAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        var ids = await dependencies.GetDependsOnIdsAsync(todoItemId, cancellationToken);
        return await GetOrderedItemsAsync(ids, cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetDependentsAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        var ids = await dependencies.GetDependentIdsAsync(todoItemId, cancellationToken);
        return await GetOrderedItemsAsync(ids, cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetAvailableDependenciesAsync(
        int todoItemId,
        CancellationToken cancellationToken = default)
    {
        await todos.GetRequiredAsync(todoItemId, cancellationToken);
        var all = await dependencies.GetAllAsync(cancellationToken);
        var excludedIds = DependencyRules.FindDependentIds(todoItemId, all)
            .Concat(all.Where(d => d.TodoItemId == todoItemId).Select(d => d.DependsOnTodoItemId))
            .Append(todoItemId)
            .ToHashSet();

        var candidates = (await todos.GetAllAsync(cancellationToken)).Where(item => !excludedIds.Contains(item.Id));
        return TodoOrdering.ByUrgency(candidates).ToList();
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

    private async Task EnsureNoCycleAsync(
        TodoItem item,
        TodoItem dependsOn,
        CancellationToken cancellationToken)
    {
        var existing = await dependencies.GetAllAsync(cancellationToken);
        DependencyRules.EnsureCanAdd(ToReference(item), ToReference(dependsOn), existing);
    }

    private async Task<IReadOnlyList<TodoItem>> GetOrderedItemsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        var items = await todos.GetByIdsAsync(ids, cancellationToken);
        return TodoOrdering.ByUrgency(items).ToList();
    }

    private static TodoReference ToReference(TodoItem item)
    {
        return new TodoReference(item.Id, item.Title);
    }

    private static FreeTextDependency FindFreeTextDependency(TodoItem item, int dependencyId)
    {
        return item.FreeTextDependencies.FirstOrDefault(dependency => dependency.Id == dependencyId)
            ?? throw new NotFoundException(EntityNames.FreeTextDependency, dependencyId);
    }
}
