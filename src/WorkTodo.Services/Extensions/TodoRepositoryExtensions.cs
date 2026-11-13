using WorkTodo.Core.Entities;
using WorkTodo.Data.Repositories;
using WorkTodo.Services.Constants;
using WorkTodo.Services.Exceptions;

namespace WorkTodo.Services.Extensions;

internal static class TodoRepositoryExtensions
{
    public static async Task<TodoItem> GetRequiredAsync(
        this ITodoRepository todos,
        int id,
        CancellationToken cancellationToken = default)
    {
        return await todos.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(EntityNames.Todo, id);
    }
}
