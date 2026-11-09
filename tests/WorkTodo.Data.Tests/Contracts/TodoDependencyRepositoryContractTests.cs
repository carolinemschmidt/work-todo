using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Exceptions;

namespace WorkTodo.Data.Tests.Contracts;

public abstract class TodoDependencyRepositoryContractTests(RepositoryStore store) : IDisposable
{
    protected RepositoryStore Store { get; } = store;

    public void Dispose() => Store.Dispose();

    [Fact]
    public async Task Added_dependency_is_returned_with_all_dependencies()
    {
        var (first, second) = await AddTwoItems();
        await Store.NewScope().Dependencies.AddAsync(new TodoDependency(first, second));

        var all = await Store.NewScope().Dependencies.GetAllAsync();

        var dependency = Assert.Single(all);
        Assert.Equal(first, dependency.TodoItemId);
        Assert.Equal(second, dependency.DependsOnTodoItemId);
    }

    [Fact]
    public async Task Deleted_dependency_is_no_longer_returned()
    {
        var (first, second) = await AddTwoItems();
        var dependency = new TodoDependency(first, second);
        var repository = Store.NewScope().Dependencies;
        await repository.AddAsync(dependency);

        await repository.DeleteAsync(dependency);

        Assert.Empty(await Store.NewScope().Dependencies.GetAllAsync());
    }

    [Fact]
    public async Task Single_dependency_is_found_by_both_ids()
    {
        var (first, second) = await AddTwoItems();
        await Store.NewScope().Dependencies.AddAsync(new TodoDependency(first, second));

        var repository = Store.NewScope().Dependencies;

        Assert.NotNull(await repository.GetAsync(first, second));
        Assert.Null(await repository.GetAsync(second, first));
    }

    [Fact]
    public async Task Ids_an_item_depends_on_exclude_other_items_dependencies()
    {
        var (first, second, third) = await AddThreeItems();
        var writer = Store.NewScope().Dependencies;
        await writer.AddAsync(new TodoDependency(first, second));
        await writer.AddAsync(new TodoDependency(third, first));

        var ids = await Store.NewScope().Dependencies.GetDependsOnIdsAsync(first);

        Assert.Equal([second], ids);
    }

    [Fact]
    public async Task Ids_of_items_waiting_on_an_item_exclude_its_own_dependencies()
    {
        var (first, second, third) = await AddThreeItems();
        var writer = Store.NewScope().Dependencies;
        await writer.AddAsync(new TodoDependency(first, second));
        await writer.AddAsync(new TodoDependency(third, first));

        var ids = await Store.NewScope().Dependencies.GetDependentIdsAsync(first);

        Assert.Equal([third], ids);
    }

    [Fact]
    public async Task Adding_the_same_dependency_twice_throws_a_data_access_exception()
    {
        var (first, second) = await AddTwoItems();
        await Store.NewScope().Dependencies.AddAsync(new TodoDependency(first, second));

        await Assert.ThrowsAsync<DataAccessException>(
            () => Store.NewScope().Dependencies.AddAsync(new TodoDependency(first, second)));
    }

    private async Task<(int First, int Second, int Third)> AddThreeItems()
    {
        var (first, second) = await AddTwoItems();
        var third = new TodoItem("Third", Priority.Low);
        await Store.NewScope().Todos.AddAsync(third);
        return (first, second, third.Id);
    }

    private async Task<(int First, int Second)> AddTwoItems()
    {
        var todos = Store.NewScope().Todos;
        var first = new TodoItem("First", Priority.Low);
        var second = new TodoItem("Second", Priority.Low);
        await todos.AddAsync(first);
        await todos.AddAsync(second);
        return (first.Id, second.Id);
    }
}
