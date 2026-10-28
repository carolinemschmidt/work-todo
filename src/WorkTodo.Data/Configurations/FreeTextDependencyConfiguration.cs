using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Configurations;

public class FreeTextDependencyConfiguration : IEntityTypeConfiguration<FreeTextDependency>
{
    public void Configure(EntityTypeBuilder<FreeTextDependency> builder)
    {
        builder.HasKey(dependency => dependency.Id);
        builder.Property(dependency => dependency.Text)
            .IsRequired()
            .HasMaxLength(ValidationConstants.FreeTextDependencyMaxLength);
    }
}
