using WorkTodo.Core.Enums;
using WorkTodo.UI.Formatting;

namespace WorkTodo.UI.Tests.Formatting;

public class EnumDisplayTests
{
    [Theory]
    [InlineData(TodoStatus.NotStarted, "Not started")]
    [InlineData(TodoStatus.InProgress, "In progress")]
    [InlineData(TodoStatus.Done, "Done")]
    public void Status_is_shown_as_readable_text(TodoStatus status, string expected)
    {
        Assert.Equal(expected, EnumDisplay.Text(status));
    }

    [Theory]
    [InlineData(Priority.Low, "Low")]
    [InlineData(Priority.Critical, "Critical")]
    public void Single_word_values_are_capitalised(Priority priority, string expected)
    {
        Assert.Equal(expected, EnumDisplay.Text(priority));
    }
}
