namespace WorkTodo.Data.Constants;

public static class DatabaseConstants
{
    public const string ConnectionStringName = "WorkTodo";
    public const int EnumColumnMaxLength = 20;
    public const string TodoItemForeignKey = "TodoItemId";
    public const string ApplyMigrationsSettingName = "Database:ApplyMigrationsOnStartup";
    public const string SaveFailedMessage = "The changes could not be saved to the database.";
    public const string MigrationFailedMessage =
        "The database could not be created or updated. Check that SQL Server is running "
        + "and that the 'WorkTodo' connection string is correct.";
}
