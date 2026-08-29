using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog> {
    public void Configure(EntityTypeBuilder<AuditLog> builder) {
        builder.ToTable("AuditLogs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Action)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(log => log.EntityName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(log => log.EntityId)
            .HasMaxLength(64);

        builder.Property(log => log.IpAddress)
            .HasMaxLength(64);

        builder.Property(log => log.UserAgent)
            .HasMaxLength(512);

        builder.HasIndex(log => log.UserId);
        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => new { log.EntityName, log.EntityId });
    }
}
