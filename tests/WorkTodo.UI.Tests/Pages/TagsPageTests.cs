using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Services;
using WorkTodo.UI.Components.Pages;

namespace WorkTodo.UI.Tests.Pages;

public class TagsPageTests : WebTestBase
{
    [Fact]
    public void Shows_a_message_when_there_are_no_tags()
    {
        var cut = Render<TagsPage>();

        cut.WaitForAssertion(() => Assert.Contains("No tags yet", cut.Markup));
    }

    [Fact]
    public async Task Added_tag_is_listed_and_saved()
    {
        var cut = Render<TagsPage>();

        cut.Find("input[aria-label='New tag name']").Input("work");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Contains("work", cut.Find(".tag-list").TextContent));
        Assert.Equal("work", Assert.Single(await GetTagsAsync()).Name);
    }

    [Fact]
    public void Blank_tag_name_shows_an_error()
    {
        var cut = Render<TagsPage>();

        cut.Find("input[aria-label='New tag name']").Input("   ");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".alert-danger")));
    }

    [Fact]
    public async Task Deleted_tag_disappears_and_is_removed_from_items_once_confirmed()
    {
        var item = await CreateTodoAsync("Write report");
        await TagAsync(item.Id, "work");
        var cut = Render<TagsPage>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("button[aria-label='Delete tag work']")));

        cut.Find("button[aria-label='Delete tag work']").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("button[aria-label='Confirm delete tag work']")));
        cut.Find("button[aria-label='Confirm delete tag work']").Click();

        cut.WaitForAssertion(() => Assert.Contains("No tags yet", cut.Markup));
        Assert.Empty((await GetTodoAsync(item.Id)).Tags);
    }

    [Fact]
    public async Task Delete_asks_first_and_says_how_many_todos_use_the_tag()
    {
        var first = await CreateTodoAsync("Write report");
        var second = await CreateTodoAsync("Get numbers");
        await TagAsync(first.Id, "work");
        await TagAsync(second.Id, "work");
        var cut = Render<TagsPage>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("button[aria-label='Delete tag work']")));

        cut.Find("button[aria-label='Delete tag work']").Click();

        cut.WaitForAssertion(() => Assert.Contains("removed from 2 todos", cut.Find(".tag-delete-confirmation").TextContent));
        Assert.Single(await GetTagsAsync());
    }

    [Fact]
    public async Task Cancelling_the_delete_keeps_the_tag()
    {
        var item = await CreateTodoAsync("Write report");
        await TagAsync(item.Id, "work");
        var cut = Render<TagsPage>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("button[aria-label='Delete tag work']")));
        cut.Find("button[aria-label='Delete tag work']").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".tag-delete-confirmation")));

        cut.Find("button[aria-label='Cancel delete tag work']").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".tag-delete-confirmation")));
        Assert.Contains("work", cut.Find(".tag-list").TextContent);
        Assert.Single(await GetTagsAsync());
    }

    [Fact]
    public async Task A_tag_that_no_todo_uses_says_so_in_the_confirmation()
    {
        await UseServicesAsync(services => services.GetRequiredService<ITagService>().GetOrCreateAsync("work"));
        var cut = Render<TagsPage>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("button[aria-label='Delete tag work']")));

        cut.Find("button[aria-label='Delete tag work']").Click();

        cut.WaitForAssertion(() => Assert.Contains("not used by any todo", cut.Find(".tag-delete-confirmation").TextContent));
    }

    private Task TagAsync(int todoId, string tagName)
    {
        return UseServicesAsync(async services =>
        {
            await services.GetRequiredService<ITodoService>().AddTagAsync(todoId, tagName);
            return 0;
        });
    }
}
