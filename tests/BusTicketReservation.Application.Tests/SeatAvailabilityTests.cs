using BusTicketReservation.Application.Services;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BusTicketReservation.Application.Tests;

public class SeatAvailabilityTests
{
    private BusTicketDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BusTicketDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new BusTicketDbContext(options);
        SeedTestData(context);
        return context;
    }

    private void SeedTestData(BusTicketDbContext context)
    {
        var route = new Route("Dhaka", "Rajshahi", 256.5m, TimeSpan.FromHours(5.5));
        context.Routes.Add(route);
        context.SaveChanges();

        var bus = new Bus("Green Line Express", "Green Line Paribahan", 40, "AC Seater");
        context.Buses.Add(bus);
        context.SaveChanges();

        var schedule = new BusSchedule(
            bus.Id,
            route.Id,
            DateTime.Today.AddHours(8),
            DateTime.Today.AddHours(13).AddMinutes(30),
            DateTime.Today,
            new Money(800, "BDT")
        );

        context.BusSchedules.Add(schedule);
        context.SaveChanges();

        // Add some existing reservations
        var reservation = new Reservation(
            schedule.Id,
            "Jane Doe",
            "jane@example.com",
            "01987654321",
            new List<int> { 1, 2 }, // Seats 1 and 2 are booked
            1600m,
            "Mohakhali",
            "Rajshahi Terminal"
        );

        context.Reservations.Add(reservation);
        context.SaveChanges();
    }

    [Fact]
    public void CheckSeatAvailability_WithAvailableSeats_ShouldReturnTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var seatService = new SeatAvailabilityService(context);
        var schedule = context.BusSchedules.First();
        var availableSeats = new List<int> { 3, 4, 5 }; // These seats should be available

        // Act
        var result = seatService.AreSeatsAvailable(schedule.Id, availableSeats);

        // Assert
        Assert.True(result.IsAvailable);
        Assert.Empty(result.UnavailableSeats);
    }

    [Fact]
    public void CheckSeatAvailability_WithBookedSeats_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var seatService = new SeatAvailabilityService(context);
        var schedule = context.BusSchedules.First();
        var requestedSeats = new List<int> { 1, 2 }; // These seats are already booked

        // Act
        var result = seatService.AreSeatsAvailable(schedule.Id, requestedSeats);

        // Assert
        Assert.False(result.IsAvailable);
        Assert.Contains(1, result.UnavailableSeats);
        Assert.Contains(2, result.UnavailableSeats);
    }

    [Fact]
    public void CheckSeatAvailability_WithMixedSeats_ShouldReturnPartialAvailability()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var seatService = new SeatAvailabilityService(context);
        var schedule = context.BusSchedules.First();
        var requestedSeats = new List<int> { 1, 3 }; // Seat 1 is booked, seat 3 is available

        // Act
        var result = seatService.AreSeatsAvailable(schedule.Id, requestedSeats);

        // Assert
        Assert.False(result.IsAvailable);
        Assert.Contains(1, result.UnavailableSeats);
        Assert.DoesNotContain(3, result.UnavailableSeats);
    }

    [Fact]
    public void GetAvailableSeats_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var seatService = new SeatAvailabilityService(context);
        var schedule = context.BusSchedules.First();
        var bus = context.Buses.First();

        // Act
        var availableSeats = seatService.GetAvailableSeats(schedule.Id);

        // Assert
        Assert.NotNull(availableSeats);
        // Total seats (40) minus booked seats (2) should be 38
        Assert.Equal(38, availableSeats.Count);
        Assert.DoesNotContain(1, availableSeats);
        Assert.DoesNotContain(2, availableSeats);
        Assert.Contains(3, availableSeats);
    }

    [Fact]
    public void ValidateSeatNumbers_WithInvalidSeatNumbers_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var seatService = new SeatAvailabilityService(context);
        var schedule = context.BusSchedules.First();
        var invalidSeats = new List<int> { 0, -1, 50 }; // Invalid seat numbers

        // Act
        var result = seatService.ValidateSeatNumbers(schedule.Id, invalidSeats);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid seat numbers", result.ErrorMessage);
    }

    [Fact]
    public void ValidateSeatNumbers_WithValidSeatNumbers_ShouldReturnTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var seatService = new SeatAvailabilityService(context);
        var schedule = context.BusSchedules.First();
        var validSeats = new List<int> { 3, 4, 5 }; // Valid seat numbers

        // Act
        var result = seatService.ValidateSeatNumbers(schedule.Id, validSeats);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
}