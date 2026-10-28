using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkTodo.Data;

namespace WorkTodo.Data.Tests;

public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
