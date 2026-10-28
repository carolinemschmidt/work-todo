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
