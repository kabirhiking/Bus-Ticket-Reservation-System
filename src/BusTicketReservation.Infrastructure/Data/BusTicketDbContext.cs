using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Infrastructure.Data.Configurations;

namespace BusTicketReservation.Infrastructure.Data;

public class BusTicketDbContext : DbContext
{
    public BusTicketDbContext(DbContextOptions<BusTicketDbContext> options) : base(options)
    {
    }

    public DbSet<Bus> Buses { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<BusSchedule> BusSchedules { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Passenger> Passengers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfiguration(new BusConfiguration());
        modelBuilder.ApplyConfiguration(new RouteConfiguration());
        modelBuilder.ApplyConfiguration(new BusScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new SeatConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new PassengerConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new OtpCodeConfiguration());

        // Configure value objects
        ConfigureValueObjects(modelBuilder);

        // Configure indexes for performance
        ConfigureIndexes(modelBuilder);

        // Configure inheritance and constraints
        ConfigureConstraints(modelBuilder);
    }

    private static void ConfigureValueObjects(ModelBuilder modelBuilder)
    {
        // Configure Money value object for BusSchedule
        modelBuilder.Entity<BusSchedule>()
            .OwnsOne(bs => bs.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Price")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

        // Configure Money value object for Ticket
        modelBuilder.Entity<Ticket>()
            .OwnsOne(t => t.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Price")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

        // Configure SeatStatus value object
        modelBuilder.Entity<Seat>()
            .Property(s => s.Status)
            .HasConversion(
                status => status.Value,
                value => SeatStatus.FromString(value))
            .HasMaxLength(20);
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Route indexes for searching
        modelBuilder.Entity<Route>()
            .HasIndex(r => new { r.FromCity, r.ToCity })
            .HasDatabaseName("IX_Routes_FromCity_ToCity");

        modelBuilder.Entity<Route>()
            .HasIndex(r => r.FromCity)
            .HasDatabaseName("IX_Routes_FromCity");

        modelBuilder.Entity<Route>()
            .HasIndex(r => r.ToCity)
            .HasDatabaseName("IX_Routes_ToCity");

        // BusSchedule indexes for searching
        modelBuilder.Entity<BusSchedule>()
            .HasIndex(bs => bs.JourneyDate)
            .HasDatabaseName("IX_BusSchedules_JourneyDate");

        modelBuilder.Entity<BusSchedule>()
            .HasIndex(bs => new { bs.RouteId, bs.JourneyDate })
            .HasDatabaseName("IX_BusSchedules_RouteId_JourneyDate");

        // Seat indexes
        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.BusId, s.Status })
            .HasDatabaseName("IX_Seats_BusId_Status");

        modelBuilder.Entity<Seat>()
            .HasIndex(s => s.SeatNumber)
            .HasDatabaseName("IX_Seats_SeatNumber");

        // Ticket indexes
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.PassengerId)
            .HasDatabaseName("IX_Tickets_PassengerId");

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.BusScheduleId)
            .HasDatabaseName("IX_Tickets_BusScheduleId");

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.SeatId, t.BusScheduleId })
            .HasDatabaseName("IX_Tickets_SeatId_BusScheduleId")
            .IsUnique();

        // Passenger indexes
        modelBuilder.Entity<Passenger>()
            .HasIndex(p => p.MobileNumber)
            .HasDatabaseName("IX_Passengers_MobileNumber")
            .IsUnique();
    }

    private static void ConfigureConstraints(ModelBuilder modelBuilder)
    {
        // Ensure seat numbers are unique within a bus
        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.BusId, s.SeatNumber })
            .HasDatabaseName("IX_Seats_BusId_SeatNumber")
            .IsUnique();

        // Ensure only one active ticket per seat per schedule
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.SeatId, t.BusScheduleId })
            .HasDatabaseName("IX_Tickets_SeatId_BusScheduleId_Unique")
            .IsUnique()
            .HasFilter("\"Status\" = 'Confirmed'");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entityEntry.Entity;
            
            if (entityEntry.State == EntityState.Added)
            {
                // CreatedAt is set in constructor, no need to update
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.GetType().GetProperty("UpdatedAt")?.SetValue(entity, DateTime.UtcNow);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}