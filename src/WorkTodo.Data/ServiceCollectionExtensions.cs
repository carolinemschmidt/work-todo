using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoDependencyRepository, TodoDependencyRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        return services;
    }
}
