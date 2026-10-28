using WorkTodo.Core.Entities;
using WorkTodo.Core.Exceptions;
using WorkTodo.Core.Rules;

namespace WorkTodo.Core.Tests.Rules;

public class DependencyRulesTests
{
    [Fact]
    public void Item_cannot_depend_on_itself()
    {
        Assert.Throws<SelfDependencyException>(() => DependencyRules.EnsureCanAdd(1, 1, []));
    }

    [Fact]
    public void Dependency_between_unrelated_items_is_allowed()
    {
        var existing = new[] { new TodoDependency(2, 3) };

        DependencyRules.EnsureCanAdd(1, 2, existing);
    }

    [Fact]
    public void Direct_cycle_is_rejected()
    {
        var existing = new[] { new TodoDependency(1, 2) };

        Assert.Throws<CircularDependencyException>(() => DependencyRules.EnsureCanAdd(2, 1, existing));
    }

    [Fact]
    public void Indirect_cycle_is_rejected()
    {
        var existing = new[] { new TodoDependency(1, 2), new TodoDependency(2, 3) };

        Assert.Throws<CircularDependencyException>(() => DependencyRules.EnsureCanAdd(3, 1, existing));
    }

    [Fact]
    public void Diamond_shaped_dependencies_are_not_a_cycle()
    {
        var existing = new[]
        {
            new TodoDependency(1, 2),
            new TodoDependency(1, 3),
            new TodoDependency(2, 4),
        };

        DependencyRules.EnsureCanAdd(3, 4, existing);
    }
}
