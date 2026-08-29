using Domain.Entities;
using Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser> {
    public void Configure(EntityTypeBuilder<ApplicationUser> builder) {
        builder
            .HasOne(user => user.Profile)
            .WithOne()
            .HasForeignKey<User>(profile => profile.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
