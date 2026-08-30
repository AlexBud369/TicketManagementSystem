using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.FirstName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.Phone)
            .HasMaxLength(20);

        builder.Property(user => user.Address)
            .HasMaxLength(200);

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder
            .HasOne(user => user.AvatarImage)
            .WithMany()
            .HasForeignKey(user => user.AvatarImageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(user => user.Orders)
            .WithOne(order => order.User)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(user => user.RefreshTokens)
            .WithOne(token => token.User)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(user => user.AuditLogs)
            .WithOne(log => log.User)
            .HasForeignKey(log => log.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
