using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WorkTodo.Data.InMemory;
using WorkTodo.Demo.Seeding;
using WorkTodo.Services;

namespace WorkTodo.Demo;

public static class DemoServiceExtensions
{
    public static IServiceCollection AddDemo(this IServiceCollection services)
    {
        services.AddInMemoryDataLayer();
        services.AddServices();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<DemoDataSeeder>();
        return services;
    }

    public static async Task SeedDemoDataAsync(this IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync();
    }
}
