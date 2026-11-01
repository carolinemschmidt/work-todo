using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;
using WorkTodo.Services.Queries;

namespace WorkTodo.Services;

public class NextUpService(ITodoRepository todos, ITodoDependencyRepository dependencies) : INextUpService
{
    public async Task<IReadOnlyList<TodoItem>> GetNextUpAsync(CancellationToken cancellationToken = default)
    {
        var items = await todos.GetAllAsync(cancellationToken);
        var allDependencies = await dependencies.GetAllAsync(cancellationToken);
        return NextUpSelector.Select(items, allDependencies);
    }
}
