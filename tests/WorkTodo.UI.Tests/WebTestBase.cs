using Bunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data;
using WorkTodo.Services;

namespace WorkTodo.UI.Tests;

public abstract class WebTestBase : BunitContext
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected WebTestBase()
    {
        _connection.Open();
        CreateSchema();
        Services.AddDataLayer(options => options.UseSqlite(_connection));
        Services.AddServices();
    }

    protected async Task<TResult> UseServicesAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
    {
        using var scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        return await action(scope.ServiceProvider);
    }

    protected Task<TodoItem> CreateTodoAsync(
        string title,
        Priority priority = Priority.Medium,
        string? description = null)
    {
        return UseServicesAsync(services =>
            services.GetRequiredService<ITodoService>().CreateAsync(title, priority, description, null));
    }

    protected Task<TodoItem> GetTodoAsync(int id)
    {
        return UseServicesAsync(services => services.GetRequiredService<ITodoService>().GetAsync(id));
    }

    protected Task<IReadOnlyList<Tag>> GetTagsAsync()
    {
        return UseServicesAsync(services => services.GetRequiredService<ITagService>().GetAllAsync());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }

    private void CreateSchema()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var context = new AppDbContext(options);
        context.Database.EnsureCreated();
    }
}
