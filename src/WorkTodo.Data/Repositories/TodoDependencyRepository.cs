using Microsoft.EntityFrameworkCore;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Repositories;

public class TodoDependencyRepository(AppDbContext context) : ITodoDependencyRepository
{
    public async Task<IReadOnlyList<TodoDependency>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TodoDependencies.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        context.TodoDependencies.Add(dependency);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoDependency dependency, CancellationToken cancellationToken = default)
    {
        context.TodoDependencies.Remove(dependency);
        await context.SaveChangesAsync(cancellationToken);
    }
}
