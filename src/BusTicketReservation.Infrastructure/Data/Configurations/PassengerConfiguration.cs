using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Infrastructure.Data.Configurations;

public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> builder)
    {
        builder.ToTable("Passengers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.MobileNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Email)
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        // Configure relationships
        builder.HasMany(p => p.Tickets)
            .WithOne(t => t.Passenger)
            .HasForeignKey(t => t.PassengerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore collections for EF tracking
        builder.Navigation(p => p.Tickets).EnableLazyLoading(false);
    }
}