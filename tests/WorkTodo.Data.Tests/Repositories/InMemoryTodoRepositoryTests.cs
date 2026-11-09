using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.Tests.Contracts;
using WorkTodo.Data.Tests.Stores;

namespace WorkTodo.Data.Tests.Repositories;

public class InMemoryTodoRepositoryTests() : TodoRepositoryContractTests(new InMemoryRepositoryStore())
{
    [Fact]
    public async Task Ticket_links_and_free_text_dependencies_get_ids_when_the_item_is_added()
    {
        var item = new TodoItem("Write report", Priority.Low);
        var link = item.AddTicketLink("https://tickets.example.com/ABC-1");
        var dependency = item.AddFreeTextDependency("Waiting for finance");

        await Store.NewScope().Todos.AddAsync(item);

        Assert.NotEqual(0, link.Id);
        Assert.NotEqual(0, dependency.Id);
    }

    [Fact]
    public async Task Ticket_links_added_later_get_distinct_ids_when_the_item_is_updated()
    {
        var item = new TodoItem("Write report", Priority.Low);
        var first = item.AddTicketLink("https://tickets.example.com/ABC-1");
        var repository = Store.NewScope().Todos;
        await repository.AddAsync(item);

        var second = item.AddTicketLink("https://tickets.example.com/ABC-2");
        await repository.UpdateAsync(item);

        Assert.NotEqual(0, second.Id);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task Update_stores_a_new_tag_added_to_the_item()
    {
        var repository = Store.NewScope().Todos;
        var item = new TodoItem("Write report", Priority.Low);
        await repository.AddAsync(item);

        item.AddTag(new Tag("work"));
        await repository.UpdateAsync(item);

        var tags = await Store.NewScope().Tags.GetAllAsync();
        Assert.Equal("work", Assert.Single(tags).Name);
    }

    [Fact]
    public async Task Adding_an_item_with_a_new_tag_whose_name_exists_throws_and_stores_nothing()
    {
        await Store.NewScope().Tags.AddAsync(new Tag("work"));
        var item = new TodoItem("Write report", Priority.Low);
        item.AddTag(new Tag("Work"));

        await Assert.ThrowsAsync<DataAccessException>(() => Store.NewScope().Todos.AddAsync(item));

        Assert.Empty(await Store.NewScope().Todos.GetAllAsync());
        Assert.Single(await Store.NewScope().Tags.GetAllAsync());
    }
}
