using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Services.Queries;

namespace WorkTodo.Services.Tests.Queries;

public class NextUpSelectorTests
{
    [Fact]
    public void Item_without_dependencies_is_next_up()
    {
        var items = new[] { TestData.CreateItem(1) };

        var result = NextUpSelector.Select(items, []);

        Assert.Single(result);
    }

    [Fact]
    public void Done_items_are_left_out()
    {
        var items = new[] { TestData.CreateItem(1, status: TodoStatus.Done) };

        var result = NextUpSelector.Select(items, []);

        Assert.Empty(result);
    }

    [Fact]
    public void Item_waiting_on_an_unfinished_item_is_left_out()
    {
        var items = new[] { TestData.CreateItem(1, "Blocked"), TestData.CreateItem(2, "Blocker") };
        var dependencies = new[] { new TodoDependency(1, 2) };

        var result = NextUpSelector.Select(items, dependencies);

        Assert.Equal("Blocker", Assert.Single(result).Title);
    }

    [Fact]
    public void Item_waiting_only_on_finished_items_is_next_up()
    {
        var items = new[]
        {
            TestData.CreateItem(1, "Ready"),
            TestData.CreateItem(2, "Finished", status: TodoStatus.Done),
        };
        var dependencies = new[] { new TodoDependency(1, 2) };

        var result = NextUpSelector.Select(items, dependencies);

        Assert.Equal("Ready", Assert.Single(result).Title);
    }

    [Fact]
    public void Item_with_an_unresolved_free_text_dependency_is_left_out()
    {
        var blocked = TestData.CreateItem(1);
        blocked.AddFreeTextDependency("Waiting for finance");

        var result = NextUpSelector.Select([blocked], []);

        Assert.Empty(result);
    }

    [Fact]
    public void Item_with_only_resolved_free_text_dependencies_is_next_up()
    {
        var ready = TestData.CreateItem(1);
        ready.AddFreeTextDependency("Waiting for finance").Resolve();

        var result = NextUpSelector.Select([ready], []);

        Assert.Single(result);
    }

    [Fact]
    public void Result_is_ordered_by_urgency()
    {
        var items = new[]
        {
            TestData.CreateItem(1, "Low", Priority.Low),
            TestData.CreateItem(2, "Critical", Priority.Critical),
        };

        var result = NextUpSelector.Select(items, []);

        Assert.Equal(["Critical", "Low"], result.Select(item => item.Title));
    }
}
