using Microsoft.EntityFrameworkCore;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Repositories;

public class TodoRepository(AppDbContext context) : ITodoRepository
{
    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await WithRelations().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await WithRelations().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        context.TodoItems.Add(item);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        var dependenciesOnItem = await context.TodoDependencies
            .Where(dependency => dependency.DependsOnTodoItemId == item.Id)
            .ToListAsync(cancellationToken);

        context.TodoDependencies.RemoveRange(dependenciesOnItem);
        context.TodoItems.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<TodoItem> WithRelations()
    {
        return context.TodoItems
            .Include(item => item.Tags)
            .Include(item => item.TicketLinks)
            .Include(item => item.FreeTextDependencies)
            .AsSplitQuery();
    }
}
