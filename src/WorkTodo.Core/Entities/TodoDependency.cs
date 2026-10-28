namespace WorkTodo.Core.Entities;

public class TodoDependency(int todoItemId, int dependsOnTodoItemId)
{
    public int TodoItemId { get; private set; } = todoItemId;

    public int DependsOnTodoItemId { get; private set; } = dependsOnTodoItemId;
}
