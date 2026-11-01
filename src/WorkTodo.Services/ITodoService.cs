using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Services.Models;

namespace WorkTodo.Services;

public interface ITodoService
{
    Task<TodoItem> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TodoItem>> GetAllAsync(TodoFilter? filter = null, CancellationToken cancellationToken = default);

    Task<TodoItem> CreateAsync(
        string title,
        Priority priority,
        string? description,
        DateOnly? dueDate,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        int id,
        string title,
        Priority priority,
        string? description,
        DateOnly? dueDate,
        CancellationToken cancellationToken = default);

    Task ChangeStatusAsync(int id, TodoStatus status, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task AddTagAsync(int id, string tagName, CancellationToken cancellationToken = default);

    Task RemoveTagAsync(int id, int tagId, CancellationToken cancellationToken = default);

    Task AddTicketLinkAsync(int id, string url, string? label, CancellationToken cancellationToken = default);

    Task RemoveTicketLinkAsync(int id, int linkId, CancellationToken cancellationToken = default);
}
