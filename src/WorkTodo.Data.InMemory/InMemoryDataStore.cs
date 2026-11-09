using WorkTodo.Core.Entities;
using WorkTodo.Data.Constants;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.InMemory.Constants;

namespace WorkTodo.Data.InMemory;

public sealed class InMemoryDataStore
{
    private readonly IdSequence _todoIds = new();
    private readonly IdSequence _tagIds = new();
    private readonly IdSequence _ticketLinkIds = new();
    private readonly IdSequence _freeTextDependencyIds = new();

    internal List<TodoItem> Todos { get; } = [];

    internal List<Tag> Tags { get; } = [];

    internal List<TodoDependency> Dependencies { get; } = [];

    internal Tag? FindTag(string name)
    {
        return Tags.FirstOrDefault(tag => HasName(tag, name));
    }

    internal void Add(TodoItem item)
    {
        var newTags = NewTagsOf(item);
        EnsureNamesAreFree(newTags);

        StoreTags(newTags);
        EntityIds.Assign(item, _todoIds.Next());
        AssignChildIds(item);
        Todos.Add(item);
    }

    internal void Update(TodoItem item)
    {
        var newTags = NewTagsOf(item);
        EnsureNamesAreFree(newTags);

        StoreTags(newTags);
        AssignChildIds(item);
    }

    internal void Remove(TodoItem item)
    {
        Dependencies.RemoveAll(dependency => dependency.TodoItemId == item.Id || dependency.DependsOnTodoItemId == item.Id);
        Todos.Remove(item);
    }

    internal void Add(Tag tag)
    {
        EnsureNamesAreFree([tag]);
        StoreTags([tag]);
    }

    internal void Remove(Tag tag)
    {
        foreach (var item in Todos)
        {
            var attached = item.Tags.FirstOrDefault(existing => existing.Id == tag.Id);
            if (attached is not null)
            {
                item.RemoveTag(attached);
            }
        }

        Tags.RemoveAll(existing => existing.Id == tag.Id);
    }

    internal void Add(TodoDependency dependency)
    {
        var exists = Dependencies.Any(existing => existing.TodoItemId == dependency.TodoItemId
            && existing.DependsOnTodoItemId == dependency.DependsOnTodoItemId);
        if (exists)
        {
            throw Failure(InMemoryConstants.DuplicateDependencyMessage);
        }

        Dependencies.Add(dependency);
    }

    private static bool HasName(Tag tag, string name)
    {
        return string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase);
    }

    private static List<Tag> NewTagsOf(TodoItem item)
    {
        return item.Tags.Where(tag => tag.Id == 0).ToList();
    }

    private static DataAccessException Failure(string reason)
    {
        return new DataAccessException(DatabaseConstants.SaveFailedMessage, new InvalidOperationException(reason));
    }

    private void EnsureNamesAreFree(IReadOnlyList<Tag> newTags)
    {
        var taken = newTags.FirstOrDefault(tag => FindTag(tag.Name) is not null);
        if (taken is not null)
        {
            throw Failure(string.Format(InMemoryConstants.DuplicateTagMessage, taken.Name));
        }
    }

    private void StoreTags(IEnumerable<Tag> newTags)
    {
        foreach (var tag in newTags)
        {
            EntityIds.Assign(tag, _tagIds.Next());
            Tags.Add(tag);
        }
    }

    private void AssignChildIds(TodoItem item)
    {
        foreach (var link in item.TicketLinks.Where(link => link.Id == 0))
        {
            EntityIds.Assign(link, _ticketLinkIds.Next());
        }

        foreach (var dependency in item.FreeTextDependencies.Where(dependency => dependency.Id == 0))
        {
            EntityIds.Assign(dependency, _freeTextDependencyIds.Next());
        }
    }
}
