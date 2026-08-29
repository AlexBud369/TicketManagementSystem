using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket> {
    public void Configure(EntityTypeBuilder<Ticket> builder) {
        builder.ToTable("Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.TicketNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(ticket => ticket.QrCode)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(ticket => ticket.SeatInfo)
            .HasMaxLength(100);

        builder.Property(ticket => ticket.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(ticket => ticket.TicketNumber)
            .IsUnique();

        builder.HasIndex(ticket => ticket.QrCode)
            .IsUnique();

        builder.HasIndex(ticket => ticket.OrderId);
        builder.HasIndex(ticket => ticket.EventId);
        builder.HasIndex(ticket => ticket.UserId);

        builder
            .HasOne(ticket => ticket.Order)
            .WithMany()
            .HasForeignKey(ticket => ticket.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(ticket => ticket.EventEntity)
            .WithMany()
            .HasForeignKey(ticket => ticket.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(ticket => ticket.User)
            .WithMany()
            .HasForeignKey(ticket => ticket.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
