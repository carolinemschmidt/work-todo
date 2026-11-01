using Microsoft.Extensions.DependencyInjection;

namespace WorkTodo.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IDependencyService, DependencyService>();
        services.AddScoped<INextUpService, NextUpService>();
        services.AddScoped<ITagService, TagService>();
        return services;
    }
}
