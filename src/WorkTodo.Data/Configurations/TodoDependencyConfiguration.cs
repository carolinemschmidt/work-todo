using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Configurations;

public class TodoDependencyConfiguration : IEntityTypeConfiguration<TodoDependency>
{
    public void Configure(EntityTypeBuilder<TodoDependency> builder)
    {
        builder.HasKey(dependency => new { dependency.TodoItemId, dependency.DependsOnTodoItemId });

        builder.HasOne<TodoItem>()
            .WithMany()
            .HasForeignKey(dependency => dependency.TodoItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQL Server rejects a second cascade path to the same table, so removing the
        // dependencies that point at a deleted item is done by TodoRepository.DeleteAsync.
        builder.HasOne<TodoItem>()
            .WithMany()
            .HasForeignKey(dependency => dependency.DependsOnTodoItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
