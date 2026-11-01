using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Services.Tests.Fakes;

internal sealed class FakeTodoRepository : ITodoRepository
{
    private readonly List<TodoItem> _items = [];
    private int _nextId = 1;
    private int _nextChildId = 1;

    public IReadOnlyList<TodoItem> Items => _items;

    public List<TodoItem> Updated { get; } = [];

    public void Seed(params TodoItem[] items)
    {
        _items.AddRange(items);
        _nextId = Math.Max(_nextId, items.Select(item => item.Id).DefaultIfEmpty(0).Max() + 1);
    }

    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
    }

    public Task<IReadOnlyList<TodoItem>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TodoItem>>(_items.Where(item => ids.Contains(item.Id)).ToList());
    }

    public Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TodoItem>>(_items.ToList());
    }

    public Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        EntityIds.Assign(item, _nextId++);
        AssignChildIds(item);
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        AssignChildIds(item);
        Updated.Add(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _items.Remove(item);
        return Task.CompletedTask;
    }

    private void AssignChildIds(TodoItem item)
    {
        foreach (var link in item.TicketLinks.Where(link => link.Id == 0))
        {
            EntityIds.Assign(link, _nextChildId++);
        }

        foreach (var dependency in item.FreeTextDependencies.Where(dependency => dependency.Id == 0))
        {
            EntityIds.Assign(dependency, _nextChildId++);
        }
    }
}
