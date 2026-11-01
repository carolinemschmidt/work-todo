namespace WorkTodo.Services.Tests;

internal static class EntityIds
{
    public static void Assign(object entity, int id)
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }
}
