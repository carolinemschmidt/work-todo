using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;
using WorkTodo.Data.Constants;

namespace WorkTodo.Data.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Title).IsRequired().HasMaxLength(ValidationConstants.TitleMaxLength);
        builder.Property(item => item.Description).HasMaxLength(ValidationConstants.DescriptionMaxLength);
        builder.Property(item => item.Priority).HasConversion<string>().HasMaxLength(DatabaseConstants.EnumColumnMaxLength);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(DatabaseConstants.EnumColumnMaxLength);

        builder.HasMany(item => item.Tags).WithMany();

        builder.HasMany(item => item.TicketLinks)
            .WithOne()
            .HasForeignKey(DatabaseConstants.TodoItemForeignKey)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(item => item.FreeTextDependencies)
            .WithOne()
            .HasForeignKey(DatabaseConstants.TodoItemForeignKey)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
