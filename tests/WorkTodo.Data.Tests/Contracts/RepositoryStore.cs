namespace WorkTodo.Data.Tests.Contracts;

public abstract class RepositoryStore : IDisposable
{
    public abstract RepositoryScope NewScope();

    public virtual void Dispose()
    {
    }
}
