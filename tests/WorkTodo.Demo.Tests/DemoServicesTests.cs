using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Demo;
using WorkTodo.Services;

namespace WorkTodo.Demo.Tests;

public class DemoServicesTests
{
    [Fact]
    public void Every_service_the_pages_need_can_be_resolved()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ITodoService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IDependencyService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<INextUpService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ITagService>());
    }

    [Fact]
    public async Task Seeded_data_is_visible_from_a_later_scope()
    {
        using var provider = BuildProvider();

        await provider.SeedDemoDataAsync();

        using var scope = provider.CreateScope();
        var items = await scope.ServiceProvider.GetRequiredService<ITodoService>().GetAllAsync();
        Assert.NotEmpty(items);
    }

    private static ServiceProvider BuildProvider()
    {
        return new ServiceCollection().AddDemo().BuildServiceProvider(validateScopes: true);
    }
}
