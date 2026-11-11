using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Data.Constants;
using WorkTodo.Data.Exceptions;

namespace WorkTodo.Data;

public static class ServiceProviderExtensions
{
    public static async Task ApplyMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (DbException exception)
        {
            throw new DataAccessException(DatabaseConstants.MigrationFailedMessage, exception);
        }
    }
}
