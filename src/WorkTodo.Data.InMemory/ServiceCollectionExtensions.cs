using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.InMemory;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryDataLayer(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryDataStore>();
        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        services.AddSingleton<ITodoDependencyRepository, InMemoryTodoDependencyRepository>();
        services.AddSingleton<ITagRepository, InMemoryTagRepository>();
        return services;
    }
}
