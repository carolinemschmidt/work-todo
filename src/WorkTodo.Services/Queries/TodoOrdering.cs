using WorkTodo.Core.Entities;

namespace WorkTodo.Services.Queries;

public static class TodoOrdering
{
    public static IOrderedEnumerable<TodoItem> ByUrgency(IEnumerable<TodoItem> items)
    {
        return items
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.DueDate is null)
            .ThenBy(item => item.DueDate)
            .ThenBy(item => item.Id);
    }
}
