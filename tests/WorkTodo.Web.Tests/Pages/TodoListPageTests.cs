using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Enums;
using WorkTodo.Services;
using WorkTodo.Web.Components.Pages;

namespace WorkTodo.Web.Tests.Pages;

public class TodoListPageTests : WebTestBase
{
    private static IEnumerable<string> Titles(IRenderedComponent<TodoListPage> cut)
    {
        return cut.FindAll(".todo-title").Select(title => title.TextContent.Trim());
    }

    [Fact]
    public async Task Shows_every_todo_most_urgent_first()
    {
        await CreateTodoAsync("Low item", Priority.Low);
        await CreateTodoAsync("Critical item", Priority.Critical);

        var cut = Render<TodoListPage>();

        cut.WaitForAssertion(() => Assert.Equal(["Critical item", "Low item"], Titles(cut)));
    }

    [Fact]
    public void Invites_to_create_the_first_todo_when_there_are_none()
    {
        var cut = Render<TodoListPage>();

        cut.WaitForAssertion(() => Assert.Contains("No todos yet", cut.Markup));
        Assert.Equal("/todos/new", cut.Find("p a").GetAttribute("href"));
    }

    [Fact]
    public async Task Says_that_filters_hide_everything_when_nothing_matches()
    {
        await CreateTodoAsync("Low item", Priority.Low);
        var cut = Render<TodoListPage>();
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".todo-item")));

        cut.Find("select[aria-label='Filter by priority']").Change("Critical");

        cut.WaitForAssertion(() => Assert.Contains("No todos match these filters", cut.Markup));
    }

    [Fact]
    public async Task Filtering_by_priority_hides_other_items()
    {
        await CreateTodoAsync("Low item", Priority.Low);
        await CreateTodoAsync("High item", Priority.High);
        var cut = Render<TodoListPage>();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".todo-item").Count));

        cut.Find("select[aria-label='Filter by priority']").Change("High");

        cut.WaitForAssertion(() => Assert.Equal(["High item"], Titles(cut)));
    }

    [Fact]
    public async Task Filtering_by_status_hides_other_items()
    {
        await CreateTodoAsync("Open item");
        var finished = await CreateTodoAsync("Finished item");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<ITodoService>().ChangeStatusAsync(finished.Id, TodoStatus.Done);
            return 0;
        });
        var cut = Render<TodoListPage>();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".todo-item").Count));

        cut.Find("select[aria-label='Filter by status']").Change("Done");

        cut.WaitForAssertion(() => Assert.Equal(["Finished item"], Titles(cut)));
    }

    [Fact]
    public async Task Filtering_by_tag_hides_untagged_items()
    {
        var tagged = await CreateTodoAsync("Tagged item");
        await CreateTodoAsync("Untagged item");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<ITodoService>().AddTagAsync(tagged.Id, "work");
            return 0;
        });
        var tagId = (await GetTagsAsync()).Single().Id;
        var cut = Render<TodoListPage>();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".todo-item").Count));

        cut.Find("select[aria-label='Filter by tag']").Change(tagId.ToString());

        cut.WaitForAssertion(() => Assert.Equal(["Tagged item"], Titles(cut)));
    }

    [Fact]
    public async Task Blocked_items_are_marked_as_blocked()
    {
        var blocked = await CreateTodoAsync("Blocked item");
        var blocker = await CreateTodoAsync("Blocker item");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<IDependencyService>().AddDependencyAsync(blocked.Id, blocker.Id);
            return 0;
        });

        var cut = Render<TodoListPage>();

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".todo-item").Count));
        var items = cut.FindAll(".todo-item");
        Assert.NotNull(items.Single(item => item.TextContent.Contains("Blocked item")).QuerySelector(".blocked-badge"));
        Assert.Null(items.Single(item => item.TextContent.Contains("Blocker item")).QuerySelector(".blocked-badge"));
    }

    [Fact]
    public async Task Marking_an_item_done_saves_the_new_status()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = Render<TodoListPage>();
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".todo-item")));

        cut.Find("button[aria-label='Mark done: Write report']").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("button[aria-label='Mark done: Write report']")));
        Assert.Equal(TodoStatus.Done, (await GetTodoAsync(item.Id)).Status);
    }
}
