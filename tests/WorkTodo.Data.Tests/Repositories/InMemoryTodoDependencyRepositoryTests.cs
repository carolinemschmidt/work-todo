using WorkTodo.Core.Entities;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.Tests.Contracts;
using WorkTodo.Data.Tests.Stores;

namespace WorkTodo.Data.Tests.Repositories;

public class InMemoryTodoDependencyRepositoryTests() : TodoDependencyRepositoryContractTests(new InMemoryRepositoryStore())
{
    [Fact]
    public async Task Adding_the_same_dependency_twice_throws_a_duplicate_entry_exception()
    {
        await Store.NewScope().Dependencies.AddAsync(new TodoDependency(1, 2));

        await Assert.ThrowsAsync<DuplicateEntryException>(
            () => Store.NewScope().Dependencies.AddAsync(new TodoDependency(1, 2)));
    }
}
