using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Exceptions;
using WorkTodo.Services;
using WorkTodo.Services.Exceptions;

namespace WorkTodo.Web.Components;

public abstract class ScopedPageBase : OwningComponentBase
{
    protected string? ErrorMessage { get; private set; }

    protected ITodoService TodoService => ScopedServices.GetRequiredService<ITodoService>();

    protected IDependencyService DependencyService => ScopedServices.GetRequiredService<IDependencyService>();

    protected INextUpService NextUpService => ScopedServices.GetRequiredService<INextUpService>();

    protected ITagService TagService => ScopedServices.GetRequiredService<ITagService>();

    protected async Task RunAsync(Func<Task> action)
    {
        ErrorMessage = null;
        try
        {
            await action();
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            ErrorMessage = exception.Message;
        }
    }

    private static bool IsExpected(Exception exception)
    {
        return exception is ArgumentException
            or NotFoundException
            or SelfDependencyException
            or CircularDependencyException;
    }
}
