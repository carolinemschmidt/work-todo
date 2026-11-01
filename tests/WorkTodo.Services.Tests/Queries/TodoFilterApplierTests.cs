using WorkTodo.Core.Enums;
using WorkTodo.Services.Models;
using WorkTodo.Services.Queries;

namespace WorkTodo.Services.Tests.Queries;

public class TodoFilterApplierTests
{
    [Fact]
    public void Empty_filter_keeps_every_item()
    {
        var items = new[] { TestData.CreateItem(1), TestData.CreateItem(2) };

        var result = TodoFilterApplier.Apply(items, new TodoFilter());

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void Filters_by_priority()
    {
        var items = new[]
        {
            TestData.CreateItem(1, "Low", Priority.Low),
            TestData.CreateItem(2, "High", Priority.High),
        };

        var result = TodoFilterApplier.Apply(items, new TodoFilter(Priority: Priority.High));

        Assert.Equal("High", Assert.Single(result).Title);
    }

    [Fact]
    public void Filters_by_status()
    {
        var items = new[]
        {
            TestData.CreateItem(1, "Open"),
            TestData.CreateItem(2, "Finished", status: TodoStatus.Done),
        };

        var result = TodoFilterApplier.Apply(items, new TodoFilter(Status: TodoStatus.Done));

        Assert.Equal("Finished", Assert.Single(result).Title);
    }

    [Fact]
    public void Filters_by_tag()
    {
        var tagged = TestData.CreateItem(1, "Tagged");
        tagged.AddTag(TestData.CreateTag(7, "work"));
        var untagged = TestData.CreateItem(2, "Untagged");

        var result = TodoFilterApplier.Apply([tagged, untagged], new TodoFilter(TagId: 7));

        Assert.Equal("Tagged", Assert.Single(result).Title);
    }

    [Fact]
    public void Combined_filters_must_all_match()
    {
        var match = TestData.CreateItem(1, "Match", Priority.High);
        match.AddTag(TestData.CreateTag(7, "work"));
        var wrongPriority = TestData.CreateItem(2, "Wrong priority", Priority.Low);
        wrongPriority.AddTag(TestData.CreateTag(7, "work"));
        var wrongTag = TestData.CreateItem(3, "Wrong tag", Priority.High);

        var result = TodoFilterApplier.Apply(
            [match, wrongPriority, wrongTag],
            new TodoFilter(TagId: 7, Priority: Priority.High));

        Assert.Equal("Match", Assert.Single(result).Title);
    }
}
