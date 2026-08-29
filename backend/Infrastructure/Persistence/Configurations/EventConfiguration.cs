using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<EventEntity> {
    public void Configure(EntityTypeBuilder<EventEntity> builder) {
        builder.ToTable("Events");

        builder.HasKey(eventEntity => eventEntity.Id);

        builder.Property(eventEntity => eventEntity.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.Description)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.Location)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.TicketPrice)
            .HasPrecision(18, 2);

        builder.Property(eventEntity => eventEntity.AvailableTickets)
            .IsConcurrencyToken();

        builder
            .HasOne(eventEntity => eventEntity.Category)
            .WithMany(category => category.Events)
            .HasForeignKey(eventEntity => eventEntity.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(eventEntity => eventEntity.Organizer)
            .WithMany()
            .HasForeignKey(eventEntity => eventEntity.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(eventEntity => eventEntity.BannerImage)
            .WithMany()
            .HasForeignKey(eventEntity => eventEntity.BannerImageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(eventEntity => eventEntity.Orders)
            .WithOne(order => order.EventEntity)
            .HasForeignKey(order => order.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(eventEntity => eventEntity.CategoryId);
        builder.HasIndex(eventEntity => eventEntity.OrganizerId);
        builder.HasIndex(eventEntity => eventEntity.Date);
        builder.HasIndex(eventEntity => eventEntity.IsPublished);
    }
}
