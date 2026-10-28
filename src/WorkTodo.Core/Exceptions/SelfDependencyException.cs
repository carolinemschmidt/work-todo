namespace WorkTodo.Core.Exceptions;

public class SelfDependencyException(int todoItemId)
    : InvalidOperationException($"Todo item {todoItemId} cannot depend on itself.");
