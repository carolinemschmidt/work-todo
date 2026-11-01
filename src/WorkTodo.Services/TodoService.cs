using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.Repositories;
using WorkTodo.Services.Exceptions;
using WorkTodo.Services.Extensions;
using WorkTodo.Services.Models;
using WorkTodo.Services.Queries;

namespace WorkTodo.Services;

public class TodoService(ITodoRepository todos, ITagService tags) : ITodoService
{
    public Task<TodoItem> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        return todos.GetRequiredAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(
        TodoFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var items = await todos.GetAllAsync(cancellationToken);
        var filtered = filter is null ? items : TodoFilterApplier.Apply(items, filter);
        return TodoOrdering.ByUrgency(filtered).ToList();
    }

    public async Task<TodoItem> CreateAsync(
        string title,
        Priority priority,
        string? description,
        DateOnly? dueDate,
        CancellationToken cancellationToken = default)
    {
        var item = new TodoItem(title, priority, description, dueDate);
        await todos.AddAsync(item, cancellationToken);
        return item;
    }

    public async Task UpdateAsync(
        int id,
        string title,
        Priority priority,
        string? description,
        DateOnly? dueDate,
        CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(id, cancellationToken);
        item.Update(title, priority, description, dueDate);
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task ChangeStatusAsync(int id, TodoStatus status, CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(id, cancellationToken);
        item.ChangeStatus(status);
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(id, cancellationToken);
        await todos.DeleteAsync(item, cancellationToken);
    }

    public async Task AddTagAsync(int id, string tagName, CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(id, cancellationToken);
        var tag = await tags.GetOrCreateAsync(tagName, cancellationToken);
        item.AddTag(tag);
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task RemoveTagAsync(int id, int tagId, CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(id, cancellationToken);
        var tag = item.Tags.FirstOrDefault(existing => existing.Id == tagId)
            ?? throw new NotFoundException(nameof(Tag), tagId);

        item.RemoveTag(tag);
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task AddTicketLinkAsync(
        int id,
        string url,
        string? label,
        CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(id, cancellationToken);
        item.AddTicketLink(url, label);
        await todos.UpdateAsync(item, cancellationToken);
    }

    public async Task RemoveTicketLinkAsync(int id, int linkId, CancellationToken cancellationToken = default)
    {
        var item = await todos.GetRequiredAsync(id, cancellationToken);
        var link = item.TicketLinks.FirstOrDefault(existing => existing.Id == linkId)
            ?? throw new NotFoundException(nameof(TicketLink), linkId);

        item.RemoveTicketLink(link);
        await todos.UpdateAsync(item, cancellationToken);
    }
}
