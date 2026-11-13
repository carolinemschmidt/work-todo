using WorkTodo.Data.Constants;

namespace WorkTodo.Data.Extensions;

internal static class SqlServerErrors
{
    public static bool IsDuplicateKey(int errorNumber)
    {
        return errorNumber is DatabaseConstants.SqlServerDuplicateIndexError
            or DatabaseConstants.SqlServerDuplicateKeyError;
    }
}
