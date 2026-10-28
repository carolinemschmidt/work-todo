using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Repositories;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);

    Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);
}
