using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Infrastructure.Data.Configurations;

public class BusScheduleConfiguration : IEntityTypeConfiguration<BusSchedule>
{
    public void Configure(EntityTypeBuilder<BusSchedule> builder)
    {
        builder.ToTable("BusSchedules");

        builder.HasKey(bs => bs.Id);

        builder.Property(bs => bs.Id)
            .ValueGeneratedNever();

        builder.Property(bs => bs.BusId)
            .IsRequired();

        builder.Property(bs => bs.RouteId)
            .IsRequired();

        builder.Property(bs => bs.DepartureTime)
            .IsRequired();

        builder.Property(bs => bs.ArrivalTime)
            .IsRequired();

        builder.Property(bs => bs.JourneyDate)
            .IsRequired();

        builder.Property(bs => bs.CreatedAt)
            .IsRequired();

        builder.Property(bs => bs.UpdatedAt);

        // Configure relationships
        builder.HasOne(bs => bs.Bus)
            .WithMany(b => b.Schedules)
            .HasForeignKey(bs => bs.BusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bs => bs.Route)
            .WithMany(r => r.BusSchedules)
            .HasForeignKey(bs => bs.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(bs => bs.Tickets)
            .WithOne(t => t.BusSchedule)
            .HasForeignKey(t => t.BusScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore collections for EF tracking
        builder.Navigation(bs => bs.Tickets).EnableLazyLoading(false);
    }
}