using WorkTodo.Core.Entities;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.Repositories;
using WorkTodo.Services.Constants;
using WorkTodo.Services.Exceptions;

namespace WorkTodo.Services;

public class TagService(ITagRepository tags) : ITagService
{
    public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return tags.GetAllAsync(cancellationToken);
    }

    public async Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var candidate = new Tag(name);
        return await tags.GetByNameAsync(candidate.Name, cancellationToken)
            ?? await AddOrFindConcurrentlyAddedAsync(candidate, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tag = await tags.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(EntityNames.Tag, id);

        await tags.DeleteAsync(tag, cancellationToken);
    }

    private async Task<Tag> AddOrFindConcurrentlyAddedAsync(Tag candidate, CancellationToken cancellationToken)
    {
        try
        {
            await tags.AddAsync(candidate, cancellationToken);
            return candidate;
        }
        catch (DataAccessException)
        {
            var concurrentlyAdded = await tags.GetByNameAsync(candidate.Name, cancellationToken);
            if (concurrentlyAdded is null)
            {
                throw;
            }

            return concurrentlyAdded;
        }
    }
}
