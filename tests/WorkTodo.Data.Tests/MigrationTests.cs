using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Data.Exceptions;

namespace WorkTodo.Data.Tests;

public class MigrationTests
{
    [Fact]
    public async Task Applying_migrations_to_an_empty_database_applies_every_migration()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        await using var provider = BuildProvider(options => options.UseSqlite(connection));

        await provider.ApplyMigrationsAsync();

        Assert.NotEmpty(await AppliedMigrationsAsync(provider));
        Assert.Empty(await PendingMigrationsAsync(provider));
    }

    [Fact]
    public async Task Applying_migrations_again_changes_nothing()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        await using var provider = BuildProvider(options => options.UseSqlite(connection));
        await provider.ApplyMigrationsAsync();
        var appliedBefore = await AppliedMigrationsAsync(provider);

        await provider.ApplyMigrationsAsync();

        Assert.Equal(appliedBefore, await AppliedMigrationsAsync(provider));
    }

    [Fact]
    public async Task A_database_that_cannot_be_reached_throws_a_data_access_exception()
    {
        var unreachable = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "todo.db");
        await using var provider = BuildProvider(options => options.UseSqlite($"Data Source={unreachable}"));

        var exception = await Assert.ThrowsAsync<DataAccessException>(() => provider.ApplyMigrationsAsync());

        Assert.NotNull(exception.InnerException);
    }

    private static async Task<IReadOnlyList<string>> AppliedMigrationsAsync(IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (await context.Database.GetAppliedMigrationsAsync()).ToList();
    }

    private static async Task<IReadOnlyList<string>> PendingMigrationsAsync(IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (await context.Database.GetPendingMigrationsAsync()).ToList();
    }

    private static ServiceProvider BuildProvider(Func<DbContextOptionsBuilder, DbContextOptionsBuilder> configureDatabase)
    {
        // The migrations are generated for SQL Server, so SQLite sees its own model as having pending changes.
        return new ServiceCollection()
            .AddDataLayer(options => configureDatabase(options)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)))
            .BuildServiceProvider();
    }
}
