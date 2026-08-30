using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem> {
    public void Configure(EntityTypeBuilder<OrderItem> builder) {
        builder.ToTable("OrderItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2);

        builder
            .HasOne(item => item.Ticket)
            .WithOne()
            .HasForeignKey<OrderItem>(item => item.TicketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(item => item.OrderId);

        builder.HasIndex(item => item.TicketId)
            .IsUnique()
            .HasFilter("[TicketId] IS NOT NULL");
    }
}
