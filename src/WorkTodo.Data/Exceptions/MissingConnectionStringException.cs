using WorkTodo.Data.Constants;

namespace WorkTodo.Data.Exceptions;

public class MissingConnectionStringException() : InvalidOperationException(DatabaseConstants.MissingConnectionStringMessage);
