using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new MissingConnectionStringException();
        }

        return services.AddDataLayer(options => options.UseSqlServer(connectionString));
    }

    public static IServiceCollection AddDataLayer(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDatabase)
    {
        services.AddDbContext<AppDbContext>(configureDatabase);
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoDependencyRepository, TodoDependencyRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        return services;
    }
}
