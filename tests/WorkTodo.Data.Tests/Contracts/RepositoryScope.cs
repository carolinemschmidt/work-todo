using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.Tests.Contracts;

public sealed record RepositoryScope(
    ITodoRepository Todos,
    ITagRepository Tags,
    ITodoDependencyRepository Dependencies);
