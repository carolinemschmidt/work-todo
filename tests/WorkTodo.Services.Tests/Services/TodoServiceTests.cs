using WorkTodo.Core.Enums;
using WorkTodo.Services.Exceptions;
using WorkTodo.Services.Models;
using WorkTodo.Services.Tests.Fakes;

namespace WorkTodo.Services.Tests.Services;

public class TodoServiceTests
{
    private readonly FakeTodoRepository _todos = new();
    private readonly FakeTagRepository _tags = new();
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        _service = new TodoService(_todos, new TagService(_tags));
    }

    [Fact]
    public async Task Created_item_is_stored_with_the_given_values()
    {
        var dueDate = new DateOnly(2026, 11, 1);

        var item = await _service.CreateAsync("Write report", Priority.High, "Numbers", dueDate);

        Assert.Contains(item, _todos.Items);
        Assert.NotEqual(0, item.Id);
        Assert.Equal("Write report", item.Title);
        Assert.Equal(Priority.High, item.Priority);
        Assert.Equal("Numbers", item.Description);
        Assert.Equal(dueDate, item.DueDate);
    }

    [Fact]
    public async Task Item_with_a_blank_title_is_not_created()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => _service.CreateAsync(" ", Priority.Low, null, null));

        Assert.Empty(_todos.Items);
    }

    [Fact]
    public async Task Existing_item_is_returned_by_id()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);

        var item = await _service.GetAsync(1);

        Assert.Same(existing, item);
    }

    [Fact]
    public async Task Getting_an_unknown_item_throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync(42));
    }

    [Fact]
    public async Task All_items_are_returned_most_urgent_first()
    {
        _todos.Seed(
            TestData.CreateItem(1, "Low", Priority.Low),
            TestData.CreateItem(2, "Critical", Priority.Critical));

        var items = await _service.GetAllAsync();

        Assert.Equal(["Critical", "Low"], items.Select(item => item.Title));
    }

    [Fact]
    public async Task All_items_can_be_filtered()
    {
        _todos.Seed(
            TestData.CreateItem(1, "Low", Priority.Low),
            TestData.CreateItem(2, "High", Priority.High));

        var items = await _service.GetAllAsync(new TodoFilter(Priority: Priority.High));

        Assert.Equal("High", Assert.Single(items).Title);
    }

    [Fact]
    public async Task Updated_item_is_changed_and_saved()
    {
        var existing = TestData.CreateItem(1, "Write report");
        _todos.Seed(existing);
        var dueDate = new DateOnly(2026, 12, 24);

        await _service.UpdateAsync(1, "Write summary", Priority.Critical, "Short", dueDate);

        Assert.Equal("Write summary", existing.Title);
        Assert.Equal(Priority.Critical, existing.Priority);
        Assert.Equal("Short", existing.Description);
        Assert.Equal(dueDate, existing.DueDate);
        Assert.Contains(existing, _todos.Updated);
    }

    [Fact]
    public async Task Updating_an_unknown_item_throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateAsync(42, "Title", Priority.Low, null, null));
    }

    [Fact]
    public async Task Changed_status_is_saved()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);

        await _service.ChangeStatusAsync(1, TodoStatus.Done);

        Assert.Equal(TodoStatus.Done, existing.Status);
        Assert.Contains(existing, _todos.Updated);
    }

    [Fact]
    public async Task Deleted_item_is_removed()
    {
        _todos.Seed(TestData.CreateItem(1));

        await _service.DeleteAsync(1);

        Assert.Empty(_todos.Items);
    }

    [Fact]
    public async Task Deleting_an_unknown_item_throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(42));
    }

    [Fact]
    public async Task Missing_tag_is_created_and_added_to_the_item()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);

        await _service.AddTagAsync(1, "work");

        Assert.Equal("work", Assert.Single(existing.Tags).Name);
        Assert.Single(_tags.Tags);
        Assert.Contains(existing, _todos.Updated);
    }

    [Fact]
    public async Task Existing_tag_is_reused_when_added_to_an_item()
    {
        var tag = TestData.CreateTag(1, "work");
        _tags.Seed(tag);
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);

        await _service.AddTagAsync(1, "work");

        Assert.Same(tag, Assert.Single(existing.Tags));
        Assert.Single(_tags.Tags);
    }

    [Fact]
    public async Task Adding_a_tag_to_an_unknown_item_throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddTagAsync(42, "work"));

        Assert.Empty(_tags.Tags);
    }

    [Fact]
    public async Task Removed_tag_leaves_the_item_but_stays_available()
    {
        var tag = TestData.CreateTag(5, "work");
        _tags.Seed(tag);
        var existing = TestData.CreateItem(1);
        existing.AddTag(tag);
        _todos.Seed(existing);

        await _service.RemoveTagAsync(1, 5);

        Assert.Empty(existing.Tags);
        Assert.Contains(tag, _tags.Tags);
        Assert.Contains(existing, _todos.Updated);
    }

    [Fact]
    public async Task Removing_a_tag_the_item_does_not_have_throws()
    {
        _todos.Seed(TestData.CreateItem(1));

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoveTagAsync(1, 5));
    }

    [Fact]
    public async Task Ticket_link_is_added_and_saved()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);

        await _service.AddTicketLinkAsync(1, "https://tickets.example.com/ABC-1", "ABC-1");

        var link = Assert.Single(existing.TicketLinks);
        Assert.Equal("ABC-1", link.Label);
        Assert.Contains(existing, _todos.Updated);
    }

    [Fact]
    public async Task Invalid_ticket_link_is_not_saved()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => _service.AddTicketLinkAsync(1, "not a url", null));

        Assert.Empty(existing.TicketLinks);
        Assert.Empty(_todos.Updated);
    }

    [Fact]
    public async Task Removed_ticket_link_is_gone()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);
        await _service.AddTicketLinkAsync(1, "https://tickets.example.com/ABC-1", null);
        var linkId = existing.TicketLinks.Single().Id;

        await _service.RemoveTicketLinkAsync(1, linkId);

        Assert.Empty(existing.TicketLinks);
    }

    [Fact]
    public async Task Removing_an_unknown_ticket_link_throws()
    {
        _todos.Seed(TestData.CreateItem(1));

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoveTicketLinkAsync(1, 99));
    }
}
