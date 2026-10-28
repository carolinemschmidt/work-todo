using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Name).IsRequired().HasMaxLength(ValidationConstants.TagNameMaxLength);
        builder.HasIndex(tag => tag.Name).IsUnique();
    }
}
