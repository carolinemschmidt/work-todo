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
        var blockedIds = FindBlockedIds(itemList, dependencies);
        var ready = itemList.Where(item => item.Status != TodoStatus.Done && !blockedIds.Contains(item.Id));

        return TodoOrdering.ByUrgency(ready).ToList();
    }

    public static IReadOnlySet<int> FindBlockedIds(
        IEnumerable<TodoItem> items,
        IEnumerable<TodoDependency> dependencies)
    {
        var itemList = items.ToList();
        var unfinishedIds = itemList
            .Where(item => item.Status != TodoStatus.Done)
            .Select(item => item.Id)
            .ToHashSet();
        var waitingOnUnfinishedIds = dependencies
            .Where(dependency => unfinishedIds.Contains(dependency.DependsOnTodoItemId))
            .Select(dependency => dependency.TodoItemId)
            .ToHashSet();

        return itemList
            .Where(item => unfinishedIds.Contains(item.Id))
            .Where(item => waitingOnUnfinishedIds.Contains(item.Id)
                || item.FreeTextDependencies.Any(dependency => !dependency.IsResolved))
            .Select(item => item.Id)
            .ToHashSet();
    }
}
