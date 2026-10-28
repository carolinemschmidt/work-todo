using Microsoft.EntityFrameworkCore;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<TodoDependency> TodoDependencies => Set<TodoDependency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
