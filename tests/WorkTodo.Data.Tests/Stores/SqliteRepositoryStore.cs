using WorkTodo.Data.Repositories;
using WorkTodo.Data.Tests.Contracts;

namespace WorkTodo.Data.Tests.Stores;

public sealed class SqliteRepositoryStore : RepositoryStore
{
    private readonly SqliteTestDatabase _database = new();

    public AppDbContext CreateContext() => _database.CreateContext();

    public override RepositoryScope NewScope()
    {
        var context = CreateContext();
        return new RepositoryScope(
            new TodoRepository(context),
            new TagRepository(context),
            new TodoDependencyRepository(context));
    }

    public override void Dispose() => _database.Dispose();
}
