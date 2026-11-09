namespace WorkTodo.UI.Constants;

public static class AppRoutes
{
    public const string Home = "/";
    public const string NextUp = "/next";
    public const string NewTodo = "/todos/new";
    public const string Tags = "/tags";

    public static string Details(int id) => $"/todos/{id}";

    public static string Edit(int id) => $"/todos/{id}/edit";
}
