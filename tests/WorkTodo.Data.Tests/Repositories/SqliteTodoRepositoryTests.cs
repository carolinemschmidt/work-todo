using Microsoft.EntityFrameworkCore;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Repositories;
using WorkTodo.Data.Tests.Contracts;
using WorkTodo.Data.Tests.Stores;

namespace WorkTodo.Data.Tests.Repositories;

public class SqliteTodoRepositoryTests : TodoRepositoryContractTests
{
    private readonly SqliteRepositoryStore _sqlite;

    public SqliteTodoRepositoryTests()
        : this(new SqliteRepositoryStore())
    {
    }

    private SqliteTodoRepositoryTests(SqliteRepositoryStore sqlite)
        : base(sqlite)
    {
        _sqlite = sqlite;
    }

    [Fact]
    public async Task Deleting_an_item_removes_its_ticket_link_and_free_text_rows()
    {
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddTicketLink("https://tickets.example.com/ABC-1");
        saved.AddFreeTextDependency("Waiting for finance");
        await new TodoRepository(_sqlite.CreateContext()).AddAsync(saved);

        var repository = new TodoRepository(_sqlite.CreateContext());
        await repository.DeleteAsync((await repository.GetByIdAsync(saved.Id))!);

        using var verification = _sqlite.CreateContext();
        Assert.Empty(await verification.Set<TicketLink>().ToListAsync());
        Assert.Empty(await verification.Set<FreeTextDependency>().ToListAsync());
    }

    [Fact]
    public async Task Removed_ticket_links_and_free_text_dependencies_are_deleted_from_their_tables()
    {
        var saved = new TodoItem("Write report", Priority.Low);
        saved.AddTicketLink("https://tickets.example.com/ABC-1");
        saved.AddFreeTextDependency("Waiting for finance");
        await new TodoRepository(_sqlite.CreateContext()).AddAsync(saved);

        var repository = new TodoRepository(_sqlite.CreateContext());
        var item = await repository.GetByIdAsync(saved.Id);
        item!.RemoveTicketLink(item.TicketLinks.Single());
        item.RemoveFreeTextDependency(item.FreeTextDependencies.Single());
        await repository.UpdateAsync(item);

        using var verification = _sqlite.CreateContext();
        Assert.Empty(await verification.Set<TicketLink>().ToListAsync());
        Assert.Empty(await verification.Set<FreeTextDependency>().ToListAsync());
    }
}
