using WorkTodo.Core.Entities;
using WorkTodo.Services.Models;

namespace WorkTodo.Services.Queries;

public static class TodoFilterApplier
{
    public static IEnumerable<TodoItem> Apply(IEnumerable<TodoItem> items, TodoFilter filter)
    {
        return items.Where(item => Matches(item, filter));
    }

    private static bool Matches(TodoItem item, TodoFilter filter)
    {
        return (filter.TagId is null || item.Tags.Any(tag => tag.Id == filter.TagId))
            && (filter.Priority is null || item.Priority == filter.Priority)
            && (filter.Status is null || item.Status == filter.Status);
    }
}
