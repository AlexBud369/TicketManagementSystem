using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ImageConfiguration : IEntityTypeConfiguration<Image> {
    public void Configure(EntityTypeBuilder<Image> builder) {
        builder.ToTable("Images");

        builder.HasKey(image => image.Id);

        builder.Property(image => image.FileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(image => image.OriginalFileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(image => image.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(image => image.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(image => image.UploadedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
