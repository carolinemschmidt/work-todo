using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Repositories;

public interface ITodoDependencyRepository
{
    Task<IReadOnlyList<TodoDependency>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(TodoDependency dependency, CancellationToken cancellationToken = default);

    Task DeleteAsync(TodoDependency dependency, CancellationToken cancellationToken = default);
}
