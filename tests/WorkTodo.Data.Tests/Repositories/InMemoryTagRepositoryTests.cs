using WorkTodo.Core.Entities;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.Tests.Contracts;
using WorkTodo.Data.Tests.Stores;

namespace WorkTodo.Data.Tests.Repositories;

public class InMemoryTagRepositoryTests() : TagRepositoryContractTests(new InMemoryRepositoryStore())
{
    [Fact]
    public async Task Tag_names_are_matched_without_regard_to_case_like_the_sql_server_default()
    {
        await Store.NewScope().Tags.AddAsync(new Tag("work"));

        var tag = await Store.NewScope().Tags.GetByNameAsync("WORK");

        Assert.Equal("work", tag!.Name);
    }

    [Fact]
    public async Task Adding_a_name_that_only_differs_in_case_throws_a_data_access_exception()
    {
        await Store.NewScope().Tags.AddAsync(new Tag("work"));

        await Assert.ThrowsAsync<DataAccessException>(
            () => Store.NewScope().Tags.AddAsync(new Tag("Work")));
    }

    [Fact]
    public async Task Added_tags_get_distinct_ids()
    {
        var first = new Tag("home");
        var second = new Tag("work");
        var repository = Store.NewScope().Tags;

        await repository.AddAsync(first);
        await repository.AddAsync(second);

        Assert.NotEqual(first.Id, second.Id);
        Assert.NotEqual(0, first.Id);
    }
}
