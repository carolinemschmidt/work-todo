using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;

namespace WorkTodo.Data.Configurations;

public class TicketLinkConfiguration : IEntityTypeConfiguration<TicketLink>
{
    public void Configure(EntityTypeBuilder<TicketLink> builder)
    {
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Url).IsRequired().HasMaxLength(ValidationConstants.TicketUrlMaxLength);
        builder.Property(link => link.Label).HasMaxLength(ValidationConstants.TicketLabelMaxLength);
    }
}
