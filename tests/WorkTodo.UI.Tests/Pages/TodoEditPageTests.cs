using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Enums;
using WorkTodo.Services;
using WorkTodo.UI.Components.Pages;

namespace WorkTodo.UI.Tests.Pages;

public class TodoEditPageTests : WebTestBase
{
    [Fact]
    public async Task New_todo_is_saved_and_the_details_page_opens()
    {
        var cut = Render<TodoEditPage>();
        cut.Find("input[aria-label='Title']").Change("Write report");
        cut.Find("textarea[aria-label='Description']").Change("Quarterly numbers");
        cut.Find("select[aria-label='Priority']").Change("High");
        cut.Find("input[aria-label='Due date']").Change("2026-12-24");

        cut.Find("form").Submit();

        var navigation = Services.GetRequiredService<NavigationManager>();
        cut.WaitForAssertion(() => Assert.Matches("/todos/\\d+$", navigation.Uri));
        var item = Assert.Single(await UseServicesAsync(services => services.GetRequiredService<ITodoService>().GetAllAsync()));
        Assert.Equal("Write report", item.Title);
        Assert.Equal("Quarterly numbers", item.Description);
        Assert.Equal(Priority.High, item.Priority);
        Assert.Equal(new DateOnly(2026, 12, 24), item.DueDate);
    }

    [Fact]
    public async Task Blank_title_shows_a_validation_message_and_saves_nothing()
    {
        var cut = Render<TodoEditPage>();

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Contains("Title is required", cut.Find(".validation-message").TextContent));
        Assert.Empty(await UseServicesAsync(services => services.GetRequiredService<ITodoService>().GetAllAsync()));
    }

    [Fact]
    public void New_todo_form_has_no_status_field()
    {
        var cut = Render<TodoEditPage>();

        Assert.Empty(cut.FindAll("select[aria-label='Status']"));
    }

    [Fact]
    public async Task Existing_todo_is_loaded_into_the_form()
    {
        var item = await CreateTodoAsync("Write report", Priority.High, "Quarterly numbers");

        var cut = Render<TodoEditPage>(p => p.Add(c => c.Id, item.Id));

        cut.WaitForAssertion(() => Assert.Equal("Write report", cut.Find("input[aria-label='Title']").GetAttribute("value")));
        Assert.Equal("Quarterly numbers", cut.Find("textarea[aria-label='Description']").GetAttribute("value"));
    }

    [Fact]
    public async Task Existing_todo_changes_are_saved()
    {
        var item = await CreateTodoAsync("Write report", Priority.Low);
        var cut = Render<TodoEditPage>(p => p.Add(c => c.Id, item.Id));
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("select[aria-label='Status']")));

        cut.Find("input[aria-label='Title']").Change("Write summary");
        cut.Find("select[aria-label='Priority']").Change("Critical");
        cut.Find("select[aria-label='Status']").Change("InProgress");
        cut.Find("form").Submit();

        var navigation = Services.GetRequiredService<NavigationManager>();
        cut.WaitForAssertion(() => Assert.EndsWith($"/todos/{item.Id}", navigation.Uri));
        var saved = await GetTodoAsync(item.Id);
        Assert.Equal("Write summary", saved.Title);
        Assert.Equal(Priority.Critical, saved.Priority);
        Assert.Equal(TodoStatus.InProgress, saved.Status);
    }

    [Fact]
    public void Unknown_todo_shows_an_error()
    {
        var cut = Render<TodoEditPage>(p => p.Add(c => c.Id, 42));

        cut.WaitForAssertion(() => Assert.Contains("was not found", cut.Find(".alert-danger").TextContent));
    }
}
