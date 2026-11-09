using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Exceptions;

namespace WorkTodo.Data.Tests.Contracts;

public abstract class TagRepositoryContractTests(RepositoryStore store) : IDisposable
{
    protected RepositoryStore Store { get; } = store;

    public void Dispose() => Store.Dispose();

    [Fact]
    public async Task Added_tag_can_be_found_by_name()
    {
        await Store.NewScope().Tags.AddAsync(new Tag("work"));

        var tag = await Store.NewScope().Tags.GetByNameAsync("work");

        Assert.NotNull(tag);
        Assert.Equal("work", tag.Name);
    }

    [Fact]
    public async Task Added_tag_can_be_found_by_id()
    {
        var saved = new Tag("work");
        await Store.NewScope().Tags.AddAsync(saved);

        var tag = await Store.NewScope().Tags.GetByIdAsync(saved.Id);

        Assert.Equal("work", tag!.Name);
    }

    [Fact]
    public async Task Unknown_id_returns_null()
    {
        var tag = await Store.NewScope().Tags.GetByIdAsync(42);

        Assert.Null(tag);
    }

    [Fact]
    public async Task Unknown_name_returns_null()
    {
        var tag = await Store.NewScope().Tags.GetByNameAsync("missing");

        Assert.Null(tag);
    }

    [Fact]
    public async Task All_tags_are_returned_ordered_by_name()
    {
        var repository = Store.NewScope().Tags;
        await repository.AddAsync(new Tag("home"));
        await repository.AddAsync(new Tag("admin"));

        var tags = await Store.NewScope().Tags.GetAllAsync();

        Assert.Equal(["admin", "home"], tags.Select(tag => tag.Name));
    }

    [Fact]
    public async Task Adding_a_duplicate_name_throws_a_data_access_exception()
    {
        await Store.NewScope().Tags.AddAsync(new Tag("work"));

        await Assert.ThrowsAsync<DataAccessException>(
            () => Store.NewScope().Tags.AddAsync(new Tag("work")));
    }

    [Fact]
    public async Task Repository_keeps_working_after_a_failed_add()
    {
        var repository = Store.NewScope().Tags;
        await repository.AddAsync(new Tag("work"));
        await Assert.ThrowsAsync<DataAccessException>(() => repository.AddAsync(new Tag("work")));

        await repository.AddAsync(new Tag("home"));

        var tags = await Store.NewScope().Tags.GetAllAsync();
        Assert.Equal(["home", "work"], tags.Select(tag => tag.Name));
    }

    [Fact]
    public async Task Deleting_a_tag_removes_it_from_items()
    {
        var scope = Store.NewScope();
        var tag = new Tag("work");
        await scope.Tags.AddAsync(tag);
        var item = new TodoItem("Write report", Priority.Low);
        item.AddTag(tag);
        await scope.Todos.AddAsync(item);

        await scope.Tags.DeleteAsync(tag);

        var reloaded = await Store.NewScope().Todos.GetByIdAsync(item.Id);
        Assert.Empty(reloaded!.Tags);
    }

    [Fact]
    public async Task Deleted_tag_is_no_longer_returned()
    {
        var scope = Store.NewScope();
        var tag = new Tag("work");
        await scope.Tags.AddAsync(tag);

        await scope.Tags.DeleteAsync(tag);

        Assert.Empty(await Store.NewScope().Tags.GetAllAsync());
    }
}
