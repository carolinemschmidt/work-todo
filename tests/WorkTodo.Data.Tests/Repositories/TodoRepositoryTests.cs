using Microsoft.EntityFrameworkCore;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.Tests.Repositories;

public class TodoRepositoryTests
{
    [Fact]
    public async Task Added_item_gets_an_id()
    {
        using var database = new SqliteTestDatabase();
        var item = new TodoItem("Write report", Priority.High);

        await new TodoRepository(database.CreateContext()).AddAsync(item);

        Assert.NotEqual(0, item.Id);
    }

    [Fact]
    public async Task Unknown_id_returns_null()
    {
        using var database = new SqliteTestDatabase();

        var item = await new TodoRepository(database.CreateContext()).GetByIdAsync(42);

        Assert.Null(item);
    }

    [Fact]
    public async Task Scalar_values_survive_a_round_trip()
    {
        using var database = new SqliteTestDatabase();
        var dueDate = new DateOnly(2026, 11, 1);
        var saved = new TodoItem("Write report", Priority.Critical, "Quarterly numbers", dueDate);
        saved.ChangeStatus(TodoStatus.InProgress);
        await new TodoRepository(database.CreateContext()).AddAsync(saved);

        var loaded = await new TodoRepository(database.CreateContext()).GetByIdAsync(saved.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Write report", loaded.Title);
        Assert.Equal(Priority.Critical, loaded.Priority);
        Assert.Equal("Quarterly numbers", loaded.Description);
        Assert.Equal(dueDate, loaded.DueDate);
        Assert.Equal(TodoStatus.InProgress, loaded.Status);
    }

    [Fact]
    public async Task Loaded_item_includes_tags_ticket_links_and_free_text_dependencies()
    {
        using var database = new SqliteTestDatabase();
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddTag(new Tag("work"));
        saved.AddTicketLink("https://tickets.example.com/ABC-1", "ABC-1");
        saved.AddFreeTextDependency("Waiting for finance");
        await new TodoRepository(database.CreateContext()).AddAsync(saved);

        var loaded = await new TodoRepository(database.CreateContext()).GetByIdAsync(saved.Id);

        Assert.NotNull(loaded);
        Assert.Equal("work", Assert.Single(loaded.Tags).Name);
        Assert.Equal("ABC-1", Assert.Single(loaded.TicketLinks).Label);
        Assert.Equal("Waiting for finance", Assert.Single(loaded.FreeTextDependencies).Text);
    }

    [Fact]
    public async Task Existing_tag_can_be_shared_between_items()
    {
        using var database = new SqliteTestDatabase();
        var context = database.CreateContext();
        var tag = new Tag("work");
        await new TagRepository(context).AddAsync(tag);
        var todos = new TodoRepository(context);
        var first = new TodoItem("First", Priority.Low);
        var second = new TodoItem("Second", Priority.Low);
        first.AddTag(tag);
        second.AddTag(tag);

        await todos.AddAsync(first);
        await todos.AddAsync(second);

        using var verification = database.CreateContext();
        Assert.Equal(1, await verification.Set<Tag>().CountAsync());
        Assert.All(await new TodoRepository(verification).GetAllAsync(), item => Assert.Single(item.Tags));
    }

    [Fact]
    public async Task All_items_are_returned()
    {
        using var database = new SqliteTestDatabase();
        var repository = new TodoRepository(database.CreateContext());
        await repository.AddAsync(new TodoItem("First", Priority.Low));
        await repository.AddAsync(new TodoItem("Second", Priority.High));

        var items = await new TodoRepository(database.CreateContext()).GetAllAsync();

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task Update_persists_changes_to_the_item_and_its_free_text_dependencies()
    {
        using var database = new SqliteTestDatabase();
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddFreeTextDependency("Waiting for finance");
        await new TodoRepository(database.CreateContext()).AddAsync(saved);

        var repository = new TodoRepository(database.CreateContext());
        var item = await repository.GetByIdAsync(saved.Id);
        item!.ChangeStatus(TodoStatus.Done);
        item.FreeTextDependencies.Single().Resolve();
        await repository.UpdateAsync(item);

        var reloaded = await new TodoRepository(database.CreateContext()).GetByIdAsync(saved.Id);
        Assert.Equal(TodoStatus.Done, reloaded!.Status);
        Assert.True(reloaded.FreeTextDependencies.Single().IsResolved);
    }

    [Fact]
    public async Task Deleting_an_item_removes_its_ticket_links_and_free_text_dependencies()
    {
        using var database = new SqliteTestDatabase();
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddTicketLink("https://tickets.example.com/ABC-1");
        saved.AddFreeTextDependency("Waiting for finance");
        await new TodoRepository(database.CreateContext()).AddAsync(saved);

        var repository = new TodoRepository(database.CreateContext());
        await repository.DeleteAsync((await repository.GetByIdAsync(saved.Id))!);

        using var verification = database.CreateContext();
        Assert.Empty(await verification.Set<TodoItem>().ToListAsync());
        Assert.Empty(await verification.Set<TicketLink>().ToListAsync());
        Assert.Empty(await verification.Set<FreeTextDependency>().ToListAsync());
    }

    [Fact]
    public async Task Deleting_an_item_removes_dependencies_in_both_directions()
    {
        using var database = new SqliteTestDatabase();
        var context = database.CreateContext();
        var todos = new TodoRepository(context);
        var first = new TodoItem("First", Priority.Low);
        var middle = new TodoItem("Middle", Priority.Low);
        var last = new TodoItem("Last", Priority.Low);
        await todos.AddAsync(first);
        await todos.AddAsync(middle);
        await todos.AddAsync(last);
        var dependencies = new TodoDependencyRepository(context);
        await dependencies.AddAsync(new TodoDependency(first.Id, middle.Id));
        await dependencies.AddAsync(new TodoDependency(middle.Id, last.Id));

        await todos.DeleteAsync(middle);

        using var verification = database.CreateContext();
        Assert.Empty(await verification.Set<TodoDependency>().ToListAsync());
        Assert.Equal(2, await verification.Set<TodoItem>().CountAsync());
    }
}
