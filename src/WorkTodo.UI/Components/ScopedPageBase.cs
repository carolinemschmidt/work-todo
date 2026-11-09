using System.Data.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkTodo.Core.Exceptions;
using WorkTodo.Data.Exceptions;
using WorkTodo.Services;
using WorkTodo.Services.Exceptions;
using WorkTodo.UI.Constants;

namespace WorkTodo.UI.Components;

public abstract class ScopedPageBase : ComponentBase, IDisposable
{
    private IServiceScope? _scope;
    private bool _isDisposed;

    protected string? ErrorMessage { get; private set; }

    [Inject]
    protected IServiceScopeFactory ScopeFactory { get; set; } = default!;

    [Inject]
    protected ILogger<ScopedPageBase> Logger { get; set; } = default!;

    protected IServiceProvider ScopedServices
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (_scope ??= ScopeFactory.CreateScope()).ServiceProvider;
        }
    }

    void IDisposable.Dispose()
    {
        _isDisposed = true;
        _scope?.Dispose();
    }

    protected ITodoService TodoService => ScopedServices.GetRequiredService<ITodoService>();

    protected IDependencyService DependencyService => ScopedServices.GetRequiredService<IDependencyService>();

    protected INextUpService NextUpService => ScopedServices.GetRequiredService<INextUpService>();

    protected ITagService TagService => ScopedServices.GetRequiredService<ITagService>();

    protected async Task<bool> RunAsync(Func<Task> action)
    {
        ErrorMessage = null;
        try
        {
            await action();
            return true;
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            ErrorMessage = exception.Message;
            return false;
        }
        catch (Exception exception) when (IsDatabaseFailure(exception))
        {
            Logger.LogError(exception, "Database operation failed");
            ErrorMessage = UiConstants.DatabaseErrorMessage;
            return false;
        }
    }

    private static bool IsExpected(Exception exception)
    {
        return exception is ArgumentException
            or NotFoundException
            or SelfDependencyException
            or CircularDependencyException;
    }

    private static bool IsDatabaseFailure(Exception exception)
    {
        return exception is DataAccessException or DbException;
    }
}
