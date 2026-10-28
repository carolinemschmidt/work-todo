namespace WorkTodo.Core.Exceptions;

public class CircularDependencyException(int todoItemId, int dependsOnTodoItemId)
    : InvalidOperationException(
        $"Todo item {todoItemId} cannot depend on {dependsOnTodoItemId} because {dependsOnTodoItemId} already depends on it.");
