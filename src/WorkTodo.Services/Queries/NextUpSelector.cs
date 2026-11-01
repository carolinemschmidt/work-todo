using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;

namespace WorkTodo.Services.Queries;

public static class NextUpSelector
{
    public static IReadOnlyList<TodoItem> Select(
        IEnumerable<TodoItem> items,
        IEnumerable<TodoDependency> dependencies)
    {
        var itemList = items.ToList();
        var unfinishedIds = itemList
            .Where(item => item.Status != TodoStatus.Done)
            .Select(item => item.Id)
            .ToHashSet();
        var blockedByItemIds = dependencies
            .Where(dependency => unfinishedIds.Contains(dependency.DependsOnTodoItemId))
            .Select(dependency => dependency.TodoItemId)
            .ToHashSet();

        var unblocked = itemList.Where(item =>
            item.Status != TodoStatus.Done
            && !blockedByItemIds.Contains(item.Id)
            && item.FreeTextDependencies.All(dependency => dependency.IsResolved));

        return TodoOrdering.ByUrgency(unblocked).ToList();
    }
}
