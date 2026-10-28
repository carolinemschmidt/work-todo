using WorkTodo.Core.Constants;
using WorkTodo.Core.Validation;

namespace WorkTodo.Core.Entities;

public class Tag
{
    public Tag(string name)
    {
        Name = Guard.RequiredText(name, ValidationConstants.TagNameMaxLength, nameof(name));
    }

    public int Id { get; private set; }

    public string Name { get; private set; }
}
