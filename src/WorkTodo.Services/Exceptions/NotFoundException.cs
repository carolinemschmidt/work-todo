namespace WorkTodo.Services.Exceptions;

public class NotFoundException(string entityName, int id)
    : Exception($"{entityName} {id} was not found. It may have been deleted.");
