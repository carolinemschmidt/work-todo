using WorkTodo.Core.Entities;
using WorkTodo.Core.Exceptions;

namespace WorkTodo.Core.Rules;

public static class DependencyRules
{
    public static void EnsureCanAdd(
        TodoReference item,
        TodoReference dependsOn,
        IEnumerable<TodoDependency> existingDependencies)
    {
        if (item.Id == dependsOn.Id)
        {
            throw new SelfDependencyException(item.Title);
        }

        if (FindDependentIds(item.Id, existingDependencies).Contains(dependsOn.Id))
        {
            throw new CircularDependencyException(item.Title, dependsOn.Title);
        }
    }

    public static IReadOnlySet<int> FindDependentIds(int todoItemId, IEnumerable<TodoDependency> dependencies)
    {
        var dependentsByItem = dependencies.ToLookup(d => d.DependsOnTodoItemId, d => d.TodoItemId);
        var found = new HashSet<int>();
        var pending = new Stack<int>([todoItemId]);

        while (pending.TryPop(out var current))
        {
            foreach (var dependent in dependentsByItem[current])
            {
                if (found.Add(dependent))
                {
                    pending.Push(dependent);
                }
            }
        }

        found.Remove(todoItemId);
        return found;
    }
}
