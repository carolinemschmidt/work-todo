using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;

namespace WorkTodo.Data.Tests.Contracts;

public abstract class TodoRepositoryContractTests(RepositoryStore store) : IDisposable
{
    protected RepositoryStore Store { get; } = store;

    public void Dispose() => Store.Dispose();

    [Fact]
    public async Task Added_item_gets_an_id()
    {
        var item = new TodoItem("Write report", Priority.High);

        await Store.NewScope().Todos.AddAsync(item);

        Assert.NotEqual(0, item.Id);
    }

    [Fact]
    public async Task Only_items_with_the_requested_ids_are_returned_with_their_relations()
    {
        var wanted = new TodoItem("Wanted", Priority.Low);
        wanted.AddTag(new Tag("work"));
        var other = new TodoItem("Other", Priority.Low);
        var writer = Store.NewScope().Todos;
        await writer.AddAsync(wanted);
        await writer.AddAsync(other);

        var items = await Store.NewScope().Todos.GetByIdsAsync([wanted.Id, 999]);

        var loaded = Assert.Single(items);
        Assert.Equal("Wanted", loaded.Title);
        Assert.Equal("work", Assert.Single(loaded.Tags).Name);
    }

    [Fact]
    public async Task No_requested_ids_returns_no_items()
    {
        await Store.NewScope().Todos.AddAsync(new TodoItem("Item", Priority.Low));

        var items = await Store.NewScope().Todos.GetByIdsAsync([]);

        Assert.Empty(items);
    }

    [Fact]
    public async Task Unknown_id_returns_null()
    {
        var item = await Store.NewScope().Todos.GetByIdAsync(42);

        Assert.Null(item);
    }

    [Fact]
    public async Task Scalar_values_survive_a_round_trip()
    {
        var dueDate = new DateOnly(2026, 11, 1);
        var saved = new TodoItem("Write report", Priority.Critical, "Quarterly numbers", dueDate);
        saved.ChangeStatus(TodoStatus.InProgress);
        await Store.NewScope().Todos.AddAsync(saved);

        var loaded = await Store.NewScope().Todos.GetByIdAsync(saved.Id);

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
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddTag(new Tag("work"));
        saved.AddTicketLink("https://tickets.example.com/ABC-1", "ABC-1");
        saved.AddFreeTextDependency("Waiting for finance");
        await Store.NewScope().Todos.AddAsync(saved);

        var loaded = await Store.NewScope().Todos.GetByIdAsync(saved.Id);

        Assert.NotNull(loaded);
        Assert.Equal("work", Assert.Single(loaded.Tags).Name);
        Assert.Equal("ABC-1", Assert.Single(loaded.TicketLinks).Label);
        Assert.Equal("Waiting for finance", Assert.Single(loaded.FreeTextDependencies).Text);
    }

    [Fact]
    public async Task Existing_tag_can_be_shared_between_items()
    {
        var scope = Store.NewScope();
        var tag = new Tag("work");
        await scope.Tags.AddAsync(tag);
        var first = new TodoItem("First", Priority.Low);
        var second = new TodoItem("Second", Priority.Low);
        first.AddTag(tag);
        second.AddTag(tag);

        await scope.Todos.AddAsync(first);
        await scope.Todos.AddAsync(second);

        var verification = Store.NewScope();
        Assert.Single(await verification.Tags.GetAllAsync());
        Assert.All(await verification.Todos.GetAllAsync(), item => Assert.Single(item.Tags));
    }

    [Fact]
    public async Task All_items_are_returned()
    {
        var repository = Store.NewScope().Todos;
        await repository.AddAsync(new TodoItem("First", Priority.Low));
        await repository.AddAsync(new TodoItem("Second", Priority.High));

        var items = await Store.NewScope().Todos.GetAllAsync();

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task Update_persists_changes_to_the_item_and_its_free_text_dependencies()
    {
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddFreeTextDependency("Waiting for finance");
        await Store.NewScope().Todos.AddAsync(saved);

        var repository = Store.NewScope().Todos;
        var item = await repository.GetByIdAsync(saved.Id);
        item!.ChangeStatus(TodoStatus.Done);
        item.FreeTextDependencies.Single().Resolve();
        await repository.UpdateAsync(item);

        var reloaded = await Store.NewScope().Todos.GetByIdAsync(saved.Id);
        Assert.Equal(TodoStatus.Done, reloaded!.Status);
        Assert.True(reloaded.FreeTextDependencies.Single().IsResolved);
    }

    [Fact]
    public async Task Deleted_item_is_no_longer_returned()
    {
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddTicketLink("https://tickets.example.com/ABC-1");
        saved.AddFreeTextDependency("Waiting for finance");
        await Store.NewScope().Todos.AddAsync(saved);

        var repository = Store.NewScope().Todos;
        await repository.DeleteAsync((await repository.GetByIdAsync(saved.Id))!);

        Assert.Empty(await Store.NewScope().Todos.GetAllAsync());
    }

    [Fact]
    public async Task Deleting_an_item_removes_dependencies_in_both_directions()
    {
        var scope = Store.NewScope();
        var first = new TodoItem("First", Priority.Low);
        var middle = new TodoItem("Middle", Priority.Low);
        var last = new TodoItem("Last", Priority.Low);
        await scope.Todos.AddAsync(first);
        await scope.Todos.AddAsync(middle);
        await scope.Todos.AddAsync(last);
        await scope.Dependencies.AddAsync(new TodoDependency(first.Id, middle.Id));
        await scope.Dependencies.AddAsync(new TodoDependency(middle.Id, last.Id));

        await scope.Todos.DeleteAsync(middle);

        var verification = Store.NewScope();
        Assert.Empty(await verification.Dependencies.GetAllAsync());
        Assert.Equal(2, (await verification.Todos.GetAllAsync()).Count);
    }

    [Fact]
    public async Task Removed_tags_ticket_links_and_free_text_dependencies_stay_removed_after_update()
    {
        var setup = Store.NewScope();
        var tag = new Tag("work");
        await setup.Tags.AddAsync(tag);
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddTag(tag);
        saved.AddTicketLink("https://tickets.example.com/ABC-1");
        saved.AddFreeTextDependency("Waiting for finance");
        await setup.Todos.AddAsync(saved);

        var repository = Store.NewScope().Todos;
        var item = await repository.GetByIdAsync(saved.Id);
        item!.RemoveTag(item.Tags.Single());
        item.RemoveTicketLink(item.TicketLinks.Single());
        item.RemoveFreeTextDependency(item.FreeTextDependencies.Single());
        await repository.UpdateAsync(item);

        var verification = Store.NewScope();
        var reloaded = await verification.Todos.GetByIdAsync(saved.Id);
        Assert.Empty(reloaded!.Tags);
        Assert.Empty(reloaded.TicketLinks);
        Assert.Empty(reloaded.FreeTextDependencies);
        Assert.Single(await verification.Tags.GetAllAsync());
    }
}
