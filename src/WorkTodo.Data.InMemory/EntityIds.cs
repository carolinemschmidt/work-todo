using System.Reflection;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.InMemory;

internal static class EntityIds
{
    private static readonly PropertyInfo TodoItemId = typeof(TodoItem).GetProperty(nameof(TodoItem.Id))!;
    private static readonly PropertyInfo TagId = typeof(Tag).GetProperty(nameof(Tag.Id))!;
    private static readonly PropertyInfo TicketLinkId = typeof(TicketLink).GetProperty(nameof(TicketLink.Id))!;
    private static readonly PropertyInfo FreeTextDependencyId =
        typeof(FreeTextDependency).GetProperty(nameof(FreeTextDependency.Id))!;

    public static void Assign(TodoItem item, int id) => TodoItemId.SetValue(item, id);

    public static void Assign(Tag tag, int id) => TagId.SetValue(tag, id);

    public static void Assign(TicketLink link, int id) => TicketLinkId.SetValue(link, id);

    public static void Assign(FreeTextDependency dependency, int id) => FreeTextDependencyId.SetValue(dependency, id);
}
