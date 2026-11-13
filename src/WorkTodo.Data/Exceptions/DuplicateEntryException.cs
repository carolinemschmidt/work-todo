using WorkTodo.Data.Constants;

namespace WorkTodo.Data.Exceptions;

public class DuplicateEntryException(Exception innerException)
    : DataAccessException(DatabaseConstants.DuplicateEntryMessage, innerException);
