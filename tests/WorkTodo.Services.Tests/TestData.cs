using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;

namespace WorkTodo.Services.Tests;

internal static class TestData
{
    public static TodoItem CreateItem(
        int id,
        string title = "Item",
        Priority priority = Priority.Medium,
        TodoStatus status = TodoStatus.NotStarted,
        DateOnly? dueDate = null)
    {
        var item = new TodoItem(title, priority, dueDate: dueDate);
        item.ChangeStatus(status);
        EntityIds.Assign(item, id);
        return item;
    }

    public static Tag CreateTag(int id, string name)
    {
        var tag = new Tag(name);
        EntityIds.Assign(tag, id);
        return tag;
    }
}
