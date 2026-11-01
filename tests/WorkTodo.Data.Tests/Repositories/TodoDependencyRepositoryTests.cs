using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.Tests.Repositories;

public class TodoDependencyRepositoryTests
{
    [Fact]
    public async Task Added_dependency_is_returned_with_all_dependencies()
    {
        using var database = new SqliteTestDatabase();
        var (first, second) = await AddTwoItems(database);
        await new TodoDependencyRepository(database.CreateContext())
            .AddAsync(new TodoDependency(first, second));

        var all = await new TodoDependencyRepository(database.CreateContext()).GetAllAsync();

        var dependency = Assert.Single(all);
        Assert.Equal(first, dependency.TodoItemId);
        Assert.Equal(second, dependency.DependsOnTodoItemId);
    }

    [Fact]
    public async Task Deleted_dependency_is_no_longer_returned()
    {
        using var database = new SqliteTestDatabase();
        var (first, second) = await AddTwoItems(database);
        var dependency = new TodoDependency(first, second);
        var repository = new TodoDependencyRepository(database.CreateContext());
        await repository.AddAsync(dependency);

        await repository.DeleteAsync(dependency);

        Assert.Empty(await new TodoDependencyRepository(database.CreateContext()).GetAllAsync());
    }

    [Fact]
    public async Task Single_dependency_is_found_by_both_ids()
    {
        using var database = new SqliteTestDatabase();
        var (first, second) = await AddTwoItems(database);
        await new TodoDependencyRepository(database.CreateContext()).AddAsync(new TodoDependency(first, second));

        var repository = new TodoDependencyRepository(database.CreateContext());

        Assert.NotNull(await repository.GetAsync(first, second));
        Assert.Null(await repository.GetAsync(second, first));
    }

    [Fact]
    public async Task Ids_an_item_depends_on_exclude_other_items_dependencies()
    {
        using var database = new SqliteTestDatabase();
        var (first, second, third) = await AddThreeItems(database);
        var writer = new TodoDependencyRepository(database.CreateContext());
        await writer.AddAsync(new TodoDependency(first, second));
        await writer.AddAsync(new TodoDependency(third, first));

        var ids = await new TodoDependencyRepository(database.CreateContext()).GetDependsOnIdsAsync(first);

        Assert.Equal([second], ids);
    }

    [Fact]
    public async Task Ids_of_items_waiting_on_an_item_exclude_its_own_dependencies()
    {
        using var database = new SqliteTestDatabase();
        var (first, second, third) = await AddThreeItems(database);
        var writer = new TodoDependencyRepository(database.CreateContext());
        await writer.AddAsync(new TodoDependency(first, second));
        await writer.AddAsync(new TodoDependency(third, first));

        var ids = await new TodoDependencyRepository(database.CreateContext()).GetDependentIdsAsync(first);

        Assert.Equal([third], ids);
    }

    private static async Task<(int First, int Second, int Third)> AddThreeItems(SqliteTestDatabase database)
    {
        var (first, second) = await AddTwoItems(database);
        var third = new TodoItem("Third", Priority.Low);
        await new TodoRepository(database.CreateContext()).AddAsync(third);
        return (first, second, third.Id);
    }

    private static async Task<(int First, int Second)> AddTwoItems(SqliteTestDatabase database)
    {
        var todos = new TodoRepository(database.CreateContext());
        var first = new TodoItem("First", Priority.Low);
        var second = new TodoItem("Second", Priority.Low);
        await todos.AddAsync(first);
        await todos.AddAsync(second);
        return (first.Id, second.Id);
    }
}
