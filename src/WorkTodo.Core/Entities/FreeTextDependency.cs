using WorkTodo.Core.Constants;
using WorkTodo.Core.Validation;

namespace WorkTodo.Core.Entities;

public class FreeTextDependency
{
    public FreeTextDependency(string text)
    {
        Text = Guard.RequiredText(text, ValidationConstants.FreeTextDependencyMaxLength, nameof(text));
    }

    public int Id { get; private set; }

    public string Text { get; private set; }

    public bool IsResolved { get; private set; }

    public void Resolve() => IsResolved = true;

    public void Reopen() => IsResolved = false;
}
