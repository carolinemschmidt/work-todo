using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Enums;
using WorkTodo.Services;
using WorkTodo.Web.Components.Pages;

namespace WorkTodo.Web.Tests.Pages;

public class TodoDetailsPageTests : WebTestBase
{
    private IRenderedComponent<TodoDetailsPage> RenderDetails(int id)
    {
        var cut = Render<TodoDetailsPage>(p => p.Add(c => c.Id, id));
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("h1")));
        return cut;
    }

    [Fact]
    public async Task Shows_the_title_and_description()
    {
        var item = await CreateTodoAsync("Write report", Priority.High, "Quarterly numbers");

        var cut = RenderDetails(item.Id);

        Assert.Equal("Write report", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Quarterly numbers", cut.Markup);
    }

    [Fact]
    public void Unknown_todo_shows_an_error()
    {
        var cut = Render<TodoDetailsPage>(p => p.Add(c => c.Id, 42));

        cut.WaitForAssertion(() => Assert.Contains("was not found", cut.Find(".alert-danger").TextContent));
    }

    [Fact]
    public async Task Status_change_is_saved()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("select[aria-label='Status']").Change("Done");

        cut.WaitForAssertion(() => Assert.Equal(TodoStatus.Done, GetTodoAsync(item.Id).Result.Status));
    }

    [Fact]
    public async Task Tag_can_be_added_and_removed()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("input[aria-label='New tag']").Input("work");
        cut.Find("form[aria-label='Add tag form']").Submit();

        cut.WaitForAssertion(() => Assert.Contains("work", cut.Find(".tag-list").TextContent));
        Assert.Equal("work", Assert.Single((await GetTodoAsync(item.Id)).Tags).Name);

        cut.Find("button[aria-label='Remove tag work']").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("button[aria-label='Remove tag work']")));
        Assert.Empty((await GetTodoAsync(item.Id)).Tags);
    }

    [Fact]
    public async Task Ticket_link_can_be_added_and_removed()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("input[aria-label='Ticket URL']").Input("https://tickets.example.com/ABC-1");
        cut.Find("input[aria-label='Ticket label']").Input("ABC-1");
        cut.Find("form[aria-label='Add ticket link form']").Submit();

        cut.WaitForAssertion(() =>
            Assert.Equal("https://tickets.example.com/ABC-1", cut.Find("a.ticket-link").GetAttribute("href")));
        Assert.Equal("ABC-1", cut.Find("a.ticket-link").TextContent.Trim());

        cut.Find("button[aria-label='Remove link ABC-1']").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("a.ticket-link")));
    }

    [Fact]
    public async Task Invalid_ticket_url_shows_an_error()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("input[aria-label='Ticket URL']").Input("not a url");
        cut.Find("form[aria-label='Add ticket link form']").Submit();

        cut.WaitForAssertion(() => Assert.Contains("http or https", cut.Find(".alert-danger").TextContent));
        Assert.Empty((await GetTodoAsync(item.Id)).TicketLinks);
    }

    [Fact]
    public async Task Failed_ticket_link_keeps_what_was_typed()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("input[aria-label='Ticket URL']").Input("not a url");
        cut.Find("form[aria-label='Add ticket link form']").Submit();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".alert-danger")));
        Assert.Equal("not a url", cut.Find("input[aria-label='Ticket URL']").GetAttribute("value"));
    }

    [Fact]
    public async Task Blank_tag_shows_a_readable_message()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("input[aria-label='New tag']").Input("   ");
        cut.Find("form[aria-label='Add tag form']").Submit();

        cut.WaitForAssertion(() => Assert.Equal("Tag name is required.", cut.Find(".alert-danger").TextContent.Trim()));
    }

    [Fact]
    public async Task Too_long_free_text_dependency_shows_an_error_and_keeps_the_text()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);
        var tooLong = new string('a', 501);

        cut.Find("input[aria-label='New free-text dependency']").Input(tooLong);
        cut.Find("form[aria-label='Add free-text dependency form']").Submit();

        cut.WaitForAssertion(() => Assert.Contains("cannot be longer than", cut.Find(".alert-danger").TextContent));
        Assert.Equal(tooLong, cut.Find("input[aria-label='New free-text dependency']").GetAttribute("value"));
    }

    [Fact]
    public async Task Dependency_on_another_item_can_be_added_and_removed()
    {
        var item = await CreateTodoAsync("Write report");
        var other = await CreateTodoAsync("Get numbers");
        var cut = RenderDetails(item.Id);

        cut.Find("select[aria-label='Depends on']").Change(other.Id.ToString());
        cut.Find("form[aria-label='Add dependency form']").Submit();

        cut.WaitForAssertion(() => Assert.Contains("Get numbers", cut.Find(".depends-on-list").TextContent));

        cut.Find("button[aria-label='Remove dependency on Get numbers']").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".depends-on-item")));
    }

    [Fact]
    public async Task Circular_dependency_shows_an_error()
    {
        var first = await CreateTodoAsync("First");
        var second = await CreateTodoAsync("Second");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<IDependencyService>().AddDependencyAsync(first.Id, second.Id);
            return 0;
        });
        var cut = RenderDetails(second.Id);

        cut.Find("select[aria-label='Depends on']").Change(first.Id.ToString());
        cut.Find("form[aria-label='Add dependency form']").Submit();

        cut.WaitForAssertion(() => Assert.Contains("already depends on it", cut.Find(".alert-danger").TextContent));
    }

    [Fact]
    public async Task Items_waiting_on_this_item_are_listed()
    {
        var waiting = await CreateTodoAsync("Waiting item");
        var blocker = await CreateTodoAsync("Blocker item");
        await UseServicesAsync(async services =>
        {
            await services.GetRequiredService<IDependencyService>().AddDependencyAsync(waiting.Id, blocker.Id);
            return 0;
        });

        var cut = RenderDetails(blocker.Id);

        Assert.Contains("Waiting item", cut.Find(".dependents-list").TextContent);
    }

    [Fact]
    public async Task Free_text_dependency_can_be_added_ticked_off_and_removed()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("input[aria-label='New free-text dependency']").Input("Waiting for finance");
        cut.Find("form[aria-label='Add free-text dependency form']").Submit();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("input[aria-label='Resolved: Waiting for finance']")));

        cut.Find("input[aria-label='Resolved: Waiting for finance']").Change(true);
        cut.WaitForAssertion(() =>
            Assert.True(GetTodoAsync(item.Id).Result.FreeTextDependencies.Single().IsResolved));

        cut.Find("input[aria-label='Resolved: Waiting for finance']").Change(false);
        cut.WaitForAssertion(() =>
            Assert.False(GetTodoAsync(item.Id).Result.FreeTextDependencies.Single().IsResolved));

        cut.Find("button[aria-label='Remove free-text dependency Waiting for finance']").Click();
        cut.WaitForAssertion(() => Assert.Empty(GetTodoAsync(item.Id).Result.FreeTextDependencies));
    }

    [Fact]
    public async Task Deleting_needs_confirmation_and_then_returns_to_the_list()
    {
        var item = await CreateTodoAsync("Write report");
        var cut = RenderDetails(item.Id);

        cut.Find("button[aria-label='Delete todo']").Click();
        Assert.NotNull(await GetTodoAsync(item.Id));
        cut.Find("button[aria-label='Confirm delete']").Click();

        cut.WaitForAssertion(() => Assert.Empty(
            UseServicesAsync(services => services.GetRequiredService<ITodoService>().GetAllAsync()).Result));
        var navigation = Services.GetRequiredService<NavigationManager>();
        Assert.Equal(navigation.BaseUri, navigation.Uri);
    }
}
