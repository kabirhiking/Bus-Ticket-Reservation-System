using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Infrastructure.Data.Configurations;

public class BusConfiguration : IEntityTypeConfiguration<Bus>
{
    public void Configure(EntityTypeBuilder<Bus> builder)
    {
        builder.ToTable("Buses");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.CompanyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.BusType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.TotalSeats)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt);

        // Configure relationships
        builder.HasMany(b => b.Seats)
            .WithOne(s => s.Bus)
            .HasForeignKey(s => s.BusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Schedules)
            .WithOne(bs => bs.Bus)
            .HasForeignKey(bs => bs.BusId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore collections for EF tracking
        builder.Navigation(b => b.Seats).EnableLazyLoading(false);
        builder.Navigation(b => b.Schedules).EnableLazyLoading(false);
    }
}