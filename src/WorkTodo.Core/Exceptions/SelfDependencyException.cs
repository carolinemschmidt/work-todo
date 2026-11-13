namespace WorkTodo.Core.Exceptions;

public class SelfDependencyException(string todoTitle)
    : InvalidOperationException($"\"{todoTitle}\" cannot depend on itself.");
