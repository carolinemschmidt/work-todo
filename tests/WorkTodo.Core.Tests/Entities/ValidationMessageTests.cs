using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;

namespace WorkTodo.Core.Tests.Entities;

public class ValidationMessageTests
{
    [Fact]
    public void Blank_title_message_names_the_field()
    {
        var exception = Assert.ThrowsAny<ArgumentException>(() => new TodoItem(" ", Priority.Low));

        Assert.Equal("Title is required.", exception.Message);
    }

    [Fact]
    public void Too_long_title_message_states_the_limit()
    {
        var title = new string('a', ValidationConstants.TitleMaxLength + 1);

        var exception = Assert.Throws<ArgumentException>(() => new TodoItem(title, Priority.Low));

        Assert.Equal($"Title cannot be longer than {ValidationConstants.TitleMaxLength} characters.", exception.Message);
    }

    [Fact]
    public void Too_long_description_message_states_the_limit()
    {
        var description = new string('a', ValidationConstants.DescriptionMaxLength + 1);

        var exception = Assert.Throws<ArgumentException>(() => new TodoItem("Title", Priority.Low, description));

        Assert.Equal(
            $"Description cannot be longer than {ValidationConstants.DescriptionMaxLength} characters.",
            exception.Message);
    }

    [Fact]
    public void Blank_tag_name_message_names_the_field()
    {
        var exception = Assert.ThrowsAny<ArgumentException>(() => new Tag(" "));

        Assert.Equal("Tag name is required.", exception.Message);
    }

    [Fact]
    public void Blank_ticket_url_message_names_the_field()
    {
        var exception = Assert.ThrowsAny<ArgumentException>(() => new TicketLink(" "));

        Assert.Equal("Ticket URL is required.", exception.Message);
    }

    [Fact]
    public void Invalid_ticket_url_message_explains_what_is_accepted()
    {
        var exception = Assert.Throws<ArgumentException>(() => new TicketLink("not a url"));

        Assert.Equal("Ticket URL must be an absolute http or https URL.", exception.Message);
    }

    [Fact]
    public void Too_long_ticket_label_message_states_the_limit()
    {
        var label = new string('a', ValidationConstants.TicketLabelMaxLength + 1);

        var exception = Assert.Throws<ArgumentException>(() => new TicketLink("https://tickets.example.com/ABC-1", label));

        Assert.Equal(
            $"Ticket label cannot be longer than {ValidationConstants.TicketLabelMaxLength} characters.",
            exception.Message);
    }

    [Fact]
    public void Blank_free_text_dependency_message_names_the_field()
    {
        var exception = Assert.ThrowsAny<ArgumentException>(() => new FreeTextDependency(" "));

        Assert.Equal("Dependency text is required.", exception.Message);
    }
}
