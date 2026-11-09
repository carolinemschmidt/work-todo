using Bunit;
using WorkTodo.UI.Components.Layout;

namespace WorkTodo.UI.Tests.Components;

public class NavMenuTests : BunitContext
{
    [Fact]
    public void Links_are_relative_to_the_base_path()
    {
        var cut = Render<NavMenu>();

        var hrefs = cut.FindAll("a[href]").Select(link => link.GetAttribute("href")).ToList();

        Assert.NotEmpty(hrefs);
        Assert.All(hrefs, href => Assert.False(href!.StartsWith('/'), $"'{href}' would leave a sub-folder app"));
    }

    [Fact]
    public void Menu_links_to_every_main_page()
    {
        var cut = Render<NavMenu>();

        var hrefs = cut.FindAll("a.nav-link").Select(link => link.GetAttribute("href"));

        Assert.Equal(["", "next", "todos/new", "tags"], hrefs);
    }
}
