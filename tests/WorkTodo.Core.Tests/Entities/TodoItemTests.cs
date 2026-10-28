using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;

namespace WorkTodo.Core.Tests.Entities;

public class TodoItemTests
{
    [Fact]
    public void New_item_starts_as_not_started()
    {
        var item = new TodoItem("Write report", Priority.Medium);

        Assert.Equal(TodoStatus.NotStarted, item.Status);
    }

    [Fact]
    public void New_item_keeps_given_values()
    {
        var dueDate = new DateOnly(2026, 11, 1);

        var item = new TodoItem("Write report", Priority.High, "Quarterly numbers", dueDate);

        Assert.Equal("Write report", item.Title);
        Assert.Equal(Priority.High, item.Priority);
        Assert.Equal("Quarterly numbers", item.Description);
        Assert.Equal(dueDate, item.DueDate);
    }

    [Fact]
    public void Title_is_trimmed()
    {
        var item = new TodoItem("  Write report  ", Priority.Low);

        Assert.Equal("Write report", item.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_title_is_rejected(string? title)
    {
        Assert.ThrowsAny<ArgumentException>(() => new TodoItem(title!, Priority.Low));
    }

    [Fact]
    public void Too_long_title_is_rejected()
    {
        var title = new string('a', ValidationConstants.TitleMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new TodoItem(title, Priority.Low));
    }

    [Fact]
    public void Too_long_description_is_rejected()
    {
        var description = new string('a', ValidationConstants.DescriptionMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new TodoItem("Write report", Priority.Low, description));
    }

    [Fact]
    public void Update_replaces_the_editable_fields()
    {
        var item = new TodoItem("Write report", Priority.Low);
        var dueDate = new DateOnly(2026, 12, 24);

        item.Update("Write summary", Priority.Critical, "Short version", dueDate);

        Assert.Equal("Write summary", item.Title);
        Assert.Equal(Priority.Critical, item.Priority);
        Assert.Equal("Short version", item.Description);
        Assert.Equal(dueDate, item.DueDate);
    }

    [Fact]
    public void Update_validates_the_title()
    {
        var item = new TodoItem("Write report", Priority.Low);

        Assert.ThrowsAny<ArgumentException>(() => item.Update(" ", Priority.Low, null, null));
    }

    [Fact]
    public void Status_can_be_changed()
    {
        var item = new TodoItem("Write report", Priority.Low);

        item.ChangeStatus(TodoStatus.Done);

        Assert.Equal(TodoStatus.Done, item.Status);
    }

    [Fact]
    public void Tag_can_be_added()
    {
        var item = new TodoItem("Write report", Priority.Low);

        item.AddTag(new Tag("work"));

        Assert.Single(item.Tags);
    }

    [Fact]
    public void Tag_with_same_name_is_not_added_twice()
    {
        var item = new TodoItem("Write report", Priority.Low);

        item.AddTag(new Tag("work"));
        item.AddTag(new Tag("WORK"));

        Assert.Single(item.Tags);
    }

    [Fact]
    public void Ticket_link_can_be_added()
    {
        var item = new TodoItem("Write report", Priority.Low);

        var link = item.AddTicketLink("https://tickets.example.com/ABC-1", "ABC-1");

        Assert.Contains(link, item.TicketLinks);
    }

    [Fact]
    public void Free_text_dependency_is_added_unresolved()
    {
        var item = new TodoItem("Write report", Priority.Low);

        var dependency = item.AddFreeTextDependency("Waiting for the numbers from finance");

        Assert.Contains(dependency, item.FreeTextDependencies);
        Assert.False(dependency.IsResolved);
    }

    [Fact]
    public void Item_can_have_many_free_text_dependencies()
    {
        var item = new TodoItem("Write report", Priority.Low);

        item.AddFreeTextDependency("Waiting for finance");
        item.AddFreeTextDependency("Need access to the dashboard");

        Assert.Equal(2, item.FreeTextDependencies.Count);
    }
}
