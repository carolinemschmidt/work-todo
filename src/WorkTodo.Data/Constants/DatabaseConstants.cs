namespace WorkTodo.Data.Constants;

public static class DatabaseConstants
{
    public const string ConnectionStringName = "WorkTodo";
    public const int EnumColumnMaxLength = 20;
    public const string TodoItemForeignKey = "TodoItemId";
    public const string ApplyMigrationsSettingName = "Database:ApplyMigrationsOnStartup";
    public const string SaveFailedMessage = "The changes could not be saved to the database.";
    public const string MissingConnectionStringMessage =
        $"Connection string '{ConnectionStringName}' is missing or empty. Set it in the app settings "
        + $"or with the environment variable ConnectionStrings__{ConnectionStringName}.";
    public const string DuplicateEntryMessage = "That already exists.";
    public const int SqlServerDuplicateIndexError = 2601;
    public const int SqlServerDuplicateKeyError = 2627;
    public const string MigrationFailedMessage =
        "The database could not be created or updated. Check that SQL Server is running "
        + $"and that the '{ConnectionStringName}' connection string is correct.";
}
