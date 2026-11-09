using WorkTodo.Data.InMemory;
using WorkTodo.Data.Tests.Contracts;

namespace WorkTodo.Data.Tests.Stores;

public sealed class InMemoryRepositoryStore : RepositoryStore
{
    private readonly InMemoryDataStore _store = new();

    public override RepositoryScope NewScope()
    {
        return new RepositoryScope(
            new InMemoryTodoRepository(_store),
            new InMemoryTagRepository(_store),
            new InMemoryTodoDependencyRepository(_store));
    }
}
