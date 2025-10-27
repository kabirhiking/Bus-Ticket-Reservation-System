using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Infrastructure.Data.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.BusId)
            .IsRequired();

        builder.Property(s => s.SeatNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.Row)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        // Configure relationships
        builder.HasOne(s => s.Bus)
            .WithMany(b => b.Seats)
            .HasForeignKey(s => s.BusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Ticket)
            .WithOne(t => t.Seat)
            .HasForeignKey<Ticket>(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}