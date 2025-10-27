using BusTicketReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusTicketReservation.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(254);

            builder.Property(u => u.FullName)
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(255);

            builder.Property(u => u.IsEmailVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            // Relationships
            builder.HasMany(u => u.Tickets)
                .WithOne()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.OtpCodes)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}