using Microsoft.EntityFrameworkCore;
using WorkTodo.Core.Entities;
using WorkTodo.Data.Extensions;

namespace WorkTodo.Data.Repositories;

public class TagRepository(AppDbContext context) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Tags.OrderBy(tag => tag.Name).ToListAsync(cancellationToken);
    }

    public async Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Tags.FirstOrDefaultAsync(tag => tag.Id == id, cancellationToken);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Tags.FirstOrDefaultAsync(tag => tag.Name == name, cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        context.Tags.Add(tag);
        await context.SaveChangesOrResetAsync(cancellationToken);
    }

    public async Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        context.Tags.Remove(tag);
        await context.SaveChangesOrResetAsync(cancellationToken);
    }
}
