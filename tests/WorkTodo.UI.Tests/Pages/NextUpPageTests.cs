using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Enums;
using WorkTodo.Services;
using WorkTodo.UI.Components.Pages;

namespace WorkTodo.UI.Tests.Pages;

public class NextUpPageTests : WebTestBase
{
    private static IEnumerable<string> Titles(IRenderedComponent<NextUpPage> cut)
    {
        return cut.FindAll(".todo-title").Select(title => title.TextContent.Trim());
    }

    [Fact]
    public void Shows_a_message_when_nothing_is_ready()
    {
        var cut = Render<NextUpPage>();

        cut.WaitForAssertion(() => Assert.Contains("Nothing is ready", cut.Markup));
    }

    [Fact]
    public async Task Shows_only_items_that_are_not_blocked()
    {
        var blocked = await CreateTodoAsync("Blocked item");
        var blocker = await CreateTodoAsync("Blocker item");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<IDependencyService>().AddDependencyAsync(blocked.Id, blocker.Id);
            return 0;
        });

        var cut = Render<NextUpPage>();

        cut.WaitForAssertion(() => Assert.Equal(["Blocker item"], Titles(cut)));
    }

    [Fact]
    public async Task Item_with_an_unresolved_free_text_dependency_is_not_shown()
    {
        var item = await CreateTodoAsync("Waiting item");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<IDependencyService>()
                .AddFreeTextDependencyAsync(item.Id, "Waiting for finance");
            return 0;
        });

        var cut = Render<NextUpPage>();

        cut.WaitForAssertion(() => Assert.Contains("Nothing is ready", cut.Markup));
    }

    [Fact]
    public async Task Finishing_the_blocker_makes_the_blocked_item_appear()
    {
        var blocked = await CreateTodoAsync("Blocked item");
        var blocker = await CreateTodoAsync("Blocker item");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<IDependencyService>().AddDependencyAsync(blocked.Id, blocker.Id);
            return 0;
        });
        var cut = Render<NextUpPage>();
        cut.WaitForAssertion(() => Assert.Equal(["Blocker item"], Titles(cut)));

        cut.Find("button[aria-label='Mark done: Blocker item']").Click();

        cut.WaitForAssertion(() => Assert.Equal(["Blocked item"], Titles(cut)));
        Assert.Equal(TodoStatus.Done, (await GetTodoAsync(blocker.Id)).Status);
    }
}
