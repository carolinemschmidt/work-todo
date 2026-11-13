namespace WorkTodo.UI.Constants;

public static class UiConstants
{
    public const string DateFormat = "yyyy-MM-dd";
    public const int NoSelection = 0;
    public const string NoDependsOnMessage = "This todo does not depend on any other todo.";
    public const string NoDependentsMessage = "Nothing is waiting on this todo.";
    public const string NoFreeTextDependenciesMessage = "Nothing else is holding this up.";
    public const string DueDateParsingMessage = "Due date must be a valid date.";
    public const string TagUnusedMessage = "This tag is not used by any todo.";
    public const string TagUsedMessage = "This tag will be removed from {0} {1}.";
    public const string TodoSingular = "todo";
    public const string TodoPlural = "todos";
    public const string DatabaseErrorMessage =
        "The database request failed. Check that SQL Server is running, then try again. More detail is in the log.";
}
