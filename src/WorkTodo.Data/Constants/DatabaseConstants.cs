namespace WorkTodo.Data.Constants;

public static class DatabaseConstants
{
    public const string ConnectionStringName = "WorkTodo";
    public const int EnumColumnMaxLength = 20;
    public const string TodoItemForeignKey = "TodoItemId";
    public const string SaveFailedMessage = "The changes could not be saved to the database.";
}
