using WorkTodo.UI.Constants;

namespace WorkTodo.UI.Tests.Constants;

public class AppRoutesTests
{
    public static TheoryData<string> AllRoutes => new()
    {
        AppRoutes.Home,
        AppRoutes.NextUp,
        AppRoutes.NewTodo,
        AppRoutes.Tags,
        AppRoutes.Details(7),
        AppRoutes.Edit(7),
    };

    [Theory]
    [MemberData(nameof(AllRoutes))]
    public void Routes_are_relative_to_the_base_path_so_the_app_can_be_hosted_in_a_sub_folder(string route)
    {
        Assert.False(route.StartsWith('/'), $"'{route}' starts with a slash and would leave a sub-folder app");
    }

    [Fact]
    public void Home_is_the_base_path_itself()
    {
        Assert.Empty(AppRoutes.Home);
    }

    [Fact]
    public void Item_routes_include_the_id()
    {
        Assert.Equal("todos/7", AppRoutes.Details(7));
        Assert.Equal("todos/7/edit", AppRoutes.Edit(7));
    }
}
