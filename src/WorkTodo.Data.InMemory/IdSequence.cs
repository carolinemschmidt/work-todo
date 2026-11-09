namespace WorkTodo.Data.InMemory;

internal sealed class IdSequence
{
    private int _last;

    public int Next() => ++_last;
}
