using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Services.Tests.Fakes;

namespace WorkTodo.Services.Tests.Services;

public class NextUpServiceTests
{
    private readonly FakeTodoRepository _todos = new();
    private readonly FakeTodoDependencyRepository _dependencies = new();
    private readonly NextUpService _service;

    public NextUpServiceTests()
    {
        _service = new NextUpService(_todos, _dependencies);
    }

    [Fact]
    public async Task Unblocked_open_items_are_returned_most_urgent_first()
    {
        _todos.Seed(
            TestData.CreateItem(1, "Low", Priority.Low),
            TestData.CreateItem(2, "Critical", Priority.Critical),
            TestData.CreateItem(3, "Finished", status: TodoStatus.Done));

        var items = await _service.GetNextUpAsync();

        Assert.Equal(["Critical", "Low"], items.Select(item => item.Title));
    }

    [Fact]
    public async Task Items_blocked_by_an_unfinished_item_are_left_out()
    {
        _todos.Seed(TestData.CreateItem(1, "Blocked"), TestData.CreateItem(2, "Blocker"));
        _dependencies.Seed(new TodoDependency(1, 2));

        var items = await _service.GetNextUpAsync();

        Assert.Equal("Blocker", Assert.Single(items).Title);
    }
}
