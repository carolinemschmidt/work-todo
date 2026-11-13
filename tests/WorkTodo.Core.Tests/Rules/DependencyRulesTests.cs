using WorkTodo.Core.Entities;
using WorkTodo.Core.Exceptions;
using WorkTodo.Core.Rules;

namespace WorkTodo.Core.Tests.Rules;

public class DependencyRulesTests
{
    [Fact]
    public void Item_cannot_depend_on_itself()
    {
        Assert.Throws<SelfDependencyException>(() => DependencyRules.EnsureCanAdd(Ref(1), Ref(1), []));
    }

    [Fact]
    public void Dependency_between_unrelated_items_is_allowed()
    {
        var existing = new[] { new TodoDependency(2, 3) };

        DependencyRules.EnsureCanAdd(Ref(1), Ref(2), existing);
    }

    [Fact]
    public void Direct_cycle_is_rejected()
    {
        var existing = new[] { new TodoDependency(1, 2) };

        Assert.Throws<CircularDependencyException>(() => DependencyRules.EnsureCanAdd(Ref(2), Ref(1), existing));
    }

    [Fact]
    public void Indirect_cycle_is_rejected()
    {
        var existing = new[] { new TodoDependency(1, 2), new TodoDependency(2, 3) };

        Assert.Throws<CircularDependencyException>(() => DependencyRules.EnsureCanAdd(Ref(3), Ref(1), existing));
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

        DependencyRules.EnsureCanAdd(Ref(3), Ref(4), existing);
    }

    [Fact]
    public void Self_dependency_message_names_the_todo()
    {
        var exception = Assert.Throws<SelfDependencyException>(
            () => DependencyRules.EnsureCanAdd(new TodoReference(1, "Write report"), new TodoReference(1, "Write report"), []));

        Assert.Equal("\"Write report\" cannot depend on itself.", exception.Message);
    }

    [Fact]
    public void Circular_dependency_message_names_both_todos()
    {
        var existing = new[] { new TodoDependency(1, 2) };

        var exception = Assert.Throws<CircularDependencyException>(
            () => DependencyRules.EnsureCanAdd(new TodoReference(2, "Get numbers"), new TodoReference(1, "Write report"), existing));

        Assert.Equal(
            "\"Get numbers\" cannot depend on \"Write report\" because \"Write report\" already depends on it, directly or through other todos.",
            exception.Message);
    }

    [Fact]
    public void Dependent_ids_include_items_that_depend_on_it_directly_and_indirectly()
    {
        var existing = new[] { new TodoDependency(2, 1), new TodoDependency(3, 2), new TodoDependency(5, 4) };

        var dependents = DependencyRules.FindDependentIds(1, existing);

        Assert.Equal([2, 3], dependents.Order());
    }

    [Fact]
    public void Dependent_ids_are_empty_when_nothing_depends_on_the_item()
    {
        var existing = new[] { new TodoDependency(1, 2) };

        Assert.Empty(DependencyRules.FindDependentIds(1, existing));
    }

    [Fact]
    public void Dependent_ids_do_not_include_the_item_itself_even_when_dependencies_loop()
    {
        var existing = new[] { new TodoDependency(2, 1), new TodoDependency(1, 2) };

        Assert.Equal([2], DependencyRules.FindDependentIds(1, existing).Order());
    }

    private static TodoReference Ref(int id)
    {
        return new TodoReference(id, $"Todo {id}");
    }
}
