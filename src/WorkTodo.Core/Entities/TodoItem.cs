using WorkTodo.Core.Constants;
using WorkTodo.Core.Enums;
using WorkTodo.Core.Validation;

namespace WorkTodo.Core.Entities;

public class TodoItem
{
    private readonly List<Tag> _tags = [];
    private readonly List<TicketLink> _ticketLinks = [];
    private readonly List<FreeTextDependency> _freeTextDependencies = [];

    private TodoItem()
    {
        Title = string.Empty;
    }

    public TodoItem(string title, Priority priority, string? description = null, DateOnly? dueDate = null)
    {
        Title = Guard.RequiredText(title, ValidationConstants.TitleMaxLength, FieldLabels.Title);
        Priority = Guard.DefinedEnum(priority, FieldLabels.Priority);
        Description = Guard.OptionalText(description, ValidationConstants.DescriptionMaxLength, FieldLabels.Description);
        DueDate = dueDate;
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public Priority Priority { get; private set; }

    public TodoStatus Status { get; private set; } = TodoStatus.NotStarted;

    public DateOnly? DueDate { get; private set; }

    public IReadOnlyCollection<Tag> Tags => _tags;

    public IReadOnlyCollection<TicketLink> TicketLinks => _ticketLinks;

    public IReadOnlyCollection<FreeTextDependency> FreeTextDependencies => _freeTextDependencies;

    public void Update(string title, Priority priority, string? description, DateOnly? dueDate)
    {
        var validTitle = Guard.RequiredText(title, ValidationConstants.TitleMaxLength, FieldLabels.Title);
        var validPriority = Guard.DefinedEnum(priority, FieldLabels.Priority);
        var validDescription = Guard.OptionalText(description, ValidationConstants.DescriptionMaxLength, FieldLabels.Description);

        Title = validTitle;
        Priority = validPriority;
        Description = validDescription;
        DueDate = dueDate;
    }

    public void ChangeStatus(TodoStatus status) => Status = Guard.DefinedEnum(status, FieldLabels.Status);

    public void AddTag(Tag tag)
    {
        if (_tags.Any(existing => string.Equals(existing.Name, tag.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _tags.Add(tag);
    }

    public void RemoveTag(Tag tag) => _tags.Remove(tag);

    public void RemoveTicketLink(TicketLink link) => _ticketLinks.Remove(link);

    public void RemoveFreeTextDependency(FreeTextDependency dependency) => _freeTextDependencies.Remove(dependency);

    public TicketLink AddTicketLink(string url, string? label = null)
    {
        var link = new TicketLink(url, label);
        _ticketLinks.Add(link);
        return link;
    }

    public FreeTextDependency AddFreeTextDependency(string text)
    {
        var dependency = new FreeTextDependency(text);
        _freeTextDependencies.Add(dependency);
        return dependency;
    }
}
