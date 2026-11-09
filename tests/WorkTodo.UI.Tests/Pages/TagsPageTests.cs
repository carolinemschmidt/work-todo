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
    public async Task Deleted_tag_disappears_and_is_removed_from_items()
    {
        var item = await CreateTodoAsync("Write report");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<ITodoService>().AddTagAsync(item.Id, "work");
            return 0;
        });
        var cut = Render<TagsPage>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("button[aria-label='Delete tag work']")));

        cut.Find("button[aria-label='Delete tag work']").Click();

        cut.WaitForAssertion(() => Assert.Contains("No tags yet", cut.Markup));
        Assert.Empty((await GetTodoAsync(item.Id)).Tags);
    }
}
