using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment> {
    public void Configure(EntityTypeBuilder<Payment> builder) {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.StripeSessionId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(payment => payment.StripePaymentIntentId)
            .HasMaxLength(255);

        builder.Property(payment => payment.StripeEventId)
            .HasMaxLength(255);

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2);

        builder.Property(payment => payment.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(payment => payment.OrderId)
            .IsUnique();

        builder.HasIndex(payment => payment.StripeSessionId)
            .IsUnique();

        builder.HasIndex(payment => payment.StripeEventId)
            .IsUnique()
            .HasFilter("[StripeEventId] IS NOT NULL");
    }
}
