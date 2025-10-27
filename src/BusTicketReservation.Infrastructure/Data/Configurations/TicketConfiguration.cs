using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.SeatId)
            .IsRequired();

        builder.Property(t => t.PassengerId)
            .IsRequired();

        builder.Property(t => t.BusScheduleId)
            .IsRequired();

        builder.Property(t => t.BoardingPoint)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.DroppingPoint)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.BookingDate)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.CancellationReason)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);

        // Configure relationships
        builder.HasOne(t => t.Seat)
            .WithOne(s => s.Ticket)
            .HasForeignKey<Ticket>(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Passenger)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.BusSchedule)
            .WithMany(bs => bs.Tickets)
            .HasForeignKey(t => t.BusScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}