using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.Tests.Repositories;

public class TagRepositoryTests
{
    [Fact]
    public async Task Added_tag_can_be_found_by_name()
    {
        using var database = new SqliteTestDatabase();
        await new TagRepository(database.CreateContext()).AddAsync(new Tag("work"));

        var tag = await new TagRepository(database.CreateContext()).GetByNameAsync("work");

        Assert.NotNull(tag);
        Assert.Equal("work", tag.Name);
    }

    [Fact]
    public async Task Unknown_name_returns_null()
    {
        using var database = new SqliteTestDatabase();

        var tag = await new TagRepository(database.CreateContext()).GetByNameAsync("missing");

        Assert.Null(tag);
    }

    [Fact]
    public async Task All_tags_are_returned_ordered_by_name()
    {
        using var database = new SqliteTestDatabase();
        var repository = new TagRepository(database.CreateContext());
        await repository.AddAsync(new Tag("home"));
        await repository.AddAsync(new Tag("admin"));

        var tags = await new TagRepository(database.CreateContext()).GetAllAsync();

        Assert.Equal(["admin", "home"], tags.Select(tag => tag.Name));
    }

    [Fact]
    public async Task Deleting_a_tag_removes_it_from_items()
    {
        using var database = new SqliteTestDatabase();
        var context = database.CreateContext();
        var tags = new TagRepository(context);
        var tag = new Tag("work");
        await tags.AddAsync(tag);
        var item = new TodoItem("Write report", Priority.Low);
        item.AddTag(tag);
        await new TodoRepository(context).AddAsync(item);

        await tags.DeleteAsync(tag);

        var reloaded = await new TodoRepository(database.CreateContext()).GetByIdAsync(item.Id);
        Assert.Empty(reloaded!.Tags);
    }
}
