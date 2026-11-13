namespace WorkTodo.Core.Exceptions;

public class CircularDependencyException(string todoTitle, string dependsOnTitle)
    : InvalidOperationException(
        $"\"{todoTitle}\" cannot depend on \"{dependsOnTitle}\" because \"{dependsOnTitle}\" already depends on it, directly or through other todos.");
