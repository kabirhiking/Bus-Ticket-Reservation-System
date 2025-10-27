using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Infrastructure.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.FromCity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.ToCity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Distance)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(r => r.EstimatedDuration)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        // Configure relationships
        builder.HasMany(r => r.BusSchedules)
            .WithOne(bs => bs.Route)
            .HasForeignKey(bs => bs.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore collections for EF tracking
        builder.Navigation(r => r.BusSchedules).EnableLazyLoading(false);
    }
}