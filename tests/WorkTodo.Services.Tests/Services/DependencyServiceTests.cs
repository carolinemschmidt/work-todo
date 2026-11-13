using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Core.Exceptions;
using WorkTodo.Services.Exceptions;
using WorkTodo.Services.Tests.Fakes;

namespace WorkTodo.Services.Tests.Services;

public class DependencyServiceTests
{
    private readonly FakeTodoRepository _todos = new();
    private readonly FakeTodoDependencyRepository _dependencies = new();
    private readonly DependencyService _service;

    public DependencyServiceTests()
    {
        _service = new DependencyService(_todos, _dependencies);
    }

    [Fact]
    public async Task Dependency_between_two_items_is_stored()
    {
        _todos.Seed(TestData.CreateItem(1), TestData.CreateItem(2));

        await _service.AddDependencyAsync(1, 2);

        var dependency = Assert.Single(_dependencies.Dependencies);
        Assert.Equal(1, dependency.TodoItemId);
        Assert.Equal(2, dependency.DependsOnTodoItemId);
    }

    [Fact]
    public async Task Item_cannot_depend_on_itself()
    {
        _todos.Seed(TestData.CreateItem(1));

        await Assert.ThrowsAsync<SelfDependencyException>(() => _service.AddDependencyAsync(1, 1));
    }

    [Fact]
    public async Task Circular_dependency_is_rejected()
    {
        _todos.Seed(TestData.CreateItem(1), TestData.CreateItem(2));
        _dependencies.Seed(new TodoDependency(1, 2));

        await Assert.ThrowsAsync<CircularDependencyException>(() => _service.AddDependencyAsync(2, 1));

        Assert.Single(_dependencies.Dependencies);
    }

    [Fact]
    public async Task Available_dependencies_leave_out_the_item_itself_and_what_it_already_depends_on()
    {
        _todos.Seed(TestData.CreateItem(1), TestData.CreateItem(2), TestData.CreateItem(3));
        _dependencies.Seed(new TodoDependency(1, 2));

        var available = await _service.GetAvailableDependenciesAsync(1);

        Assert.Equal([3], available.Select(item => item.Id));
    }

    [Fact]
    public async Task Available_dependencies_leave_out_items_that_would_create_a_cycle()
    {
        _todos.Seed(
            TestData.CreateItem(1),
            TestData.CreateItem(2),
            TestData.CreateItem(3),
            TestData.CreateItem(4));
        _dependencies.Seed(new TodoDependency(2, 1), new TodoDependency(3, 2));

        var available = await _service.GetAvailableDependenciesAsync(1);

        Assert.Equal([4], available.Select(item => item.Id));
    }

    [Fact]
    public async Task Available_dependencies_are_ordered_most_urgent_first()
    {
        _todos.Seed(
            TestData.CreateItem(1),
            TestData.CreateItem(2, "Low one", Priority.Low),
            TestData.CreateItem(3, "Critical one", Priority.Critical));

        var available = await _service.GetAvailableDependenciesAsync(1);

        Assert.Equal([3, 2], available.Select(item => item.Id));
    }

    [Fact]
    public async Task Available_dependencies_for_an_unknown_item_is_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAvailableDependenciesAsync(42));
    }

    [Fact]
    public async Task Rejected_circular_dependency_names_both_todos()
    {
        _todos.Seed(TestData.CreateItem(1, "Write report"), TestData.CreateItem(2, "Get numbers"));
        _dependencies.Seed(new TodoDependency(1, 2));

        var exception = await Assert.ThrowsAsync<CircularDependencyException>(() => _service.AddDependencyAsync(2, 1));

        Assert.Contains("\"Get numbers\"", exception.Message);
        Assert.Contains("\"Write report\"", exception.Message);
    }

    [Fact]
    public async Task Rejected_self_dependency_names_the_todo()
    {
        _todos.Seed(TestData.CreateItem(1, "Write report"));

        var exception = await Assert.ThrowsAsync<SelfDependencyException>(() => _service.AddDependencyAsync(1, 1));

        Assert.Contains("\"Write report\"", exception.Message);
    }

    [Fact]
    public async Task Adding_the_same_dependency_twice_keeps_one()
    {
        _todos.Seed(TestData.CreateItem(1), TestData.CreateItem(2));

        await _service.AddDependencyAsync(1, 2);
        await _service.AddDependencyAsync(1, 2);

        Assert.Single(_dependencies.Dependencies);
    }

    [Theory]
    [InlineData(1, 42)]
    [InlineData(42, 1)]
    public async Task Dependency_on_an_unknown_item_throws(int todoItemId, int dependsOnTodoItemId)
    {
        _todos.Seed(TestData.CreateItem(1));

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.AddDependencyAsync(todoItemId, dependsOnTodoItemId));
    }

    [Fact]
    public async Task Removed_dependency_is_gone()
    {
        _dependencies.Seed(new TodoDependency(1, 2), new TodoDependency(1, 3));

        await _service.RemoveDependencyAsync(1, 2);

        var remaining = Assert.Single(_dependencies.Dependencies);
        Assert.Equal(3, remaining.DependsOnTodoItemId);
    }

    [Fact]
    public async Task Removing_a_dependency_that_does_not_exist_does_nothing()
    {
        _dependencies.Seed(new TodoDependency(1, 2));

        await _service.RemoveDependencyAsync(2, 1);

        Assert.Single(_dependencies.Dependencies);
    }

    [Fact]
    public async Task Items_an_item_depends_on_are_returned()
    {
        _todos.Seed(TestData.CreateItem(1, "A"), TestData.CreateItem(2, "B"), TestData.CreateItem(3, "C"));
        _dependencies.Seed(new TodoDependency(1, 2), new TodoDependency(3, 1));

        var dependsOn = await _service.GetDependsOnAsync(1);

        Assert.Equal("B", Assert.Single(dependsOn).Title);
    }

    [Fact]
    public async Task Items_waiting_on_an_item_are_returned()
    {
        _todos.Seed(TestData.CreateItem(1, "A"), TestData.CreateItem(2, "B"), TestData.CreateItem(3, "C"));
        _dependencies.Seed(new TodoDependency(1, 2), new TodoDependency(3, 1));

        var dependents = await _service.GetDependentsAsync(1);

        Assert.Equal("C", Assert.Single(dependents).Title);
    }

    [Fact]
    public async Task Free_text_dependency_is_added_unresolved_and_saved()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);

        await _service.AddFreeTextDependencyAsync(1, "Waiting for finance");

        var dependency = Assert.Single(existing.FreeTextDependencies);
        Assert.Equal("Waiting for finance", dependency.Text);
        Assert.False(dependency.IsResolved);
        Assert.Contains(existing, _todos.Updated);
    }

    [Fact]
    public async Task Free_text_dependency_on_an_unknown_item_throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddFreeTextDependencyAsync(42, "Text"));
    }

    [Fact]
    public async Task Free_text_dependency_can_be_ticked_off_and_reopened()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);
        await _service.AddFreeTextDependencyAsync(1, "Waiting for finance");
        var dependency = existing.FreeTextDependencies.Single();

        await _service.ResolveFreeTextDependencyAsync(1, dependency.Id);
        Assert.True(dependency.IsResolved);

        await _service.ReopenFreeTextDependencyAsync(1, dependency.Id);
        Assert.False(dependency.IsResolved);
    }

    [Fact]
    public async Task Resolving_saves_the_item()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);
        await _service.AddFreeTextDependencyAsync(1, "Waiting for finance");
        _todos.Updated.Clear();

        await _service.ResolveFreeTextDependencyAsync(1, existing.FreeTextDependencies.Single().Id);

        Assert.Contains(existing, _todos.Updated);
    }

    [Fact]
    public async Task Removed_free_text_dependency_is_gone()
    {
        var existing = TestData.CreateItem(1);
        _todos.Seed(existing);
        await _service.AddFreeTextDependencyAsync(1, "Waiting for finance");

        await _service.RemoveFreeTextDependencyAsync(1, existing.FreeTextDependencies.Single().Id);

        Assert.Empty(existing.FreeTextDependencies);
    }

    [Fact]
    public async Task Unknown_free_text_dependency_throws()
    {
        _todos.Seed(TestData.CreateItem(1));

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ResolveFreeTextDependencyAsync(1, 99));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ReopenFreeTextDependencyAsync(1, 99));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoveFreeTextDependencyAsync(1, 99));
    }
}
