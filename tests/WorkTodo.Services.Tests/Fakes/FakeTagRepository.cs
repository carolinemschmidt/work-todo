using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Services.Tests.Fakes;

internal sealed class FakeTagRepository : ITagRepository
{
    private readonly List<Tag> _tags = [];
    private int _nextId = 1;

    public IReadOnlyList<Tag> Tags => _tags;

    public void Seed(params Tag[] tags)
    {
        _tags.AddRange(tags);
        _nextId = Math.Max(_nextId, tags.Select(tag => tag.Id).DefaultIfEmpty(0).Max() + 1);
    }

    public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Tag>>(_tags.OrderBy(tag => tag.Name).ToList());
    }

    public Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tags.FirstOrDefault(tag => tag.Id == id));
    }

    public Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tags.FirstOrDefault(
            tag => string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        EntityIds.Assign(tag, _nextId++);
        _tags.Add(tag);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _tags.Remove(tag);
        return Task.CompletedTask;
    }
}
