using BusTicketReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusTicketReservation.Infrastructure.Data.Configurations
{
    public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(EntityTypeBuilder<OtpCode> builder)
        {
            builder.ToTable("OtpCodes");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(254);

            builder.Property(o => o.Code)
                .IsRequired()
                .HasMaxLength(6)
                .IsFixedLength();

            builder.Property(o => o.Purpose)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(o => o.ExpiresAt)
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.AttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(o => o.MaxAttempts)
                .IsRequired()
                .HasDefaultValue(3);

            // Indexes
            builder.HasIndex(o => new { o.Email, o.Purpose, o.IsUsed })
                .HasDatabaseName("IX_OtpCodes_Email_Purpose_IsUsed");

            builder.HasIndex(o => o.ExpiresAt)
                .HasDatabaseName("IX_OtpCodes_ExpiresAt");

            builder.HasIndex(o => new { o.Email, o.Purpose, o.ExpiresAt })
                .HasDatabaseName("IX_OtpCodes_Email_Purpose_ExpiresAt");

            // Relationships
            builder.HasOne(o => o.User)
                .WithMany(u => u.OtpCodes)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}