using WorkTodo.Core.Entities;
using WorkTodo.Core.Exceptions;

namespace WorkTodo.Core.Rules;

public static class DependencyRules
{
    public static void EnsureCanAdd(
        int todoItemId,
        int dependsOnTodoItemId,
        IEnumerable<TodoDependency> existingDependencies)
    {
        if (todoItemId == dependsOnTodoItemId)
        {
            throw new SelfDependencyException(todoItemId);
        }

        if (LeadsTo(dependsOnTodoItemId, todoItemId, existingDependencies))
        {
            throw new CircularDependencyException(todoItemId, dependsOnTodoItemId);
        }
    }

    private static bool LeadsTo(int startId, int targetId, IEnumerable<TodoDependency> dependencies)
    {
        var dependsOnByItem = dependencies.ToLookup(d => d.TodoItemId, d => d.DependsOnTodoItemId);
        var visited = new HashSet<int>();
        var pending = new Stack<int>([startId]);

        while (pending.TryPop(out var current))
        {
            if (current == targetId)
            {
                return true;
            }

            if (!visited.Add(current))
            {
                continue;
            }

            foreach (var next in dependsOnByItem[current])
            {
                pending.Push(next);
            }
        }

        return false;
    }
}
