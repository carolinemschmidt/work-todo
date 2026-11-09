using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Data.InMemory;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.Tests;

public class InMemoryServiceCollectionExtensionsTests
{
    [Fact]
    public void Every_repository_interface_can_be_resolved()
    {
        using var provider = BuildProvider();

        Assert.IsType<InMemoryTodoRepository>(provider.GetRequiredService<ITodoRepository>());
        Assert.IsType<InMemoryTagRepository>(provider.GetRequiredService<ITagRepository>());
        Assert.IsType<InMemoryTodoDependencyRepository>(provider.GetRequiredService<ITodoDependencyRepository>());
    }

    [Fact]
    public async Task Data_is_shared_between_scopes()
    {
        using var provider = BuildProvider();
        using (var writing = provider.CreateScope())
        {
            await writing.ServiceProvider.GetRequiredService<ITodoRepository>()
                .AddAsync(new TodoItem("Write report", Priority.Low));
        }

        using var reading = provider.CreateScope();
        var items = await reading.ServiceProvider.GetRequiredService<ITodoRepository>().GetAllAsync();

        Assert.Equal("Write report", Assert.Single(items).Title);
    }

    private static ServiceProvider BuildProvider()
    {
        return new ServiceCollection().AddInMemoryDataLayer().BuildServiceProvider(validateScopes: true);
    }
}
