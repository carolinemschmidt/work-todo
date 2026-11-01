using WorkTodo.Core.Enums;
using WorkTodo.Services.Queries;

namespace WorkTodo.Services.Tests.Queries;

public class TodoOrderingTests
{
    [Fact]
    public void Higher_priority_comes_first()
    {
        var items = new[]
        {
            TestData.CreateItem(1, "Low", Priority.Low),
            TestData.CreateItem(2, "Critical", Priority.Critical),
            TestData.CreateItem(3, "High", Priority.High),
        };

        var ordered = TodoOrdering.ByUrgency(items);

        Assert.Equal(["Critical", "High", "Low"], ordered.Select(item => item.Title));
    }

    [Fact]
    public void Earlier_due_date_comes_first_within_a_priority()
    {
        var items = new[]
        {
            TestData.CreateItem(1, "Later", dueDate: new DateOnly(2026, 12, 1)),
            TestData.CreateItem(2, "Sooner", dueDate: new DateOnly(2026, 11, 1)),
        };

        var ordered = TodoOrdering.ByUrgency(items);

        Assert.Equal(["Sooner", "Later"], ordered.Select(item => item.Title));
    }

    [Fact]
    public void Items_without_due_date_come_last_within_a_priority()
    {
        var items = new[]
        {
            TestData.CreateItem(1, "No date"),
            TestData.CreateItem(2, "Dated", dueDate: new DateOnly(2026, 12, 1)),
        };

        var ordered = TodoOrdering.ByUrgency(items);

        Assert.Equal(["Dated", "No date"], ordered.Select(item => item.Title));
    }

    [Fact]
    public void Id_breaks_remaining_ties()
    {
        var items = new[]
        {
            TestData.CreateItem(2, "Second"),
            TestData.CreateItem(1, "First"),
        };

        var ordered = TodoOrdering.ByUrgency(items);

        Assert.Equal(["First", "Second"], ordered.Select(item => item.Title));
    }
}
