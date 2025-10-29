using BusTicketReservation.Application.Services;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BusTicketReservation.Application.Tests;

public class BookingServiceTests
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
    }

    [Fact]
    public void GetSeatPlan_WithValidScheduleId_ShouldReturnSeatPlan()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var bookingService = new BookingService(context);
        var schedule = context.BusSchedules.First();

        // Act
        var seatPlan = bookingService.GetSeatPlan(schedule.Id);

        // Assert
        Assert.NotNull(seatPlan);
        Assert.Equal(schedule.Id, seatPlan.ScheduleId);
        Assert.True(seatPlan.TotalSeats > 0);
        Assert.NotNull(seatPlan.BoardingPoints);
        Assert.NotNull(seatPlan.DroppingPoints);
    }

    [Fact]
    public void GetSeatPlan_WithInvalidScheduleId_ShouldReturnNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var bookingService = new BookingService(context);
        var invalidScheduleId = 999;

        // Act
        var seatPlan = bookingService.GetSeatPlan(invalidScheduleId);

        // Assert
        Assert.Null(seatPlan);
    }

    [Fact]
    public void BookSeats_WithValidData_ShouldCreateReservation()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var bookingService = new BookingService(context);
        var schedule = context.BusSchedules.First();
        
        var bookingRequest = new BookSeatRequest
        {
            ScheduleId = schedule.Id,
            PassengerName = "John Doe",
            PassengerEmail = "john@example.com",
            PassengerPhone = "01712345678",
            SeatNumbers = new List<int> { 1, 2 },
            BoardingPoint = "Mohakhali",
            DroppingPoint = "Rajshahi Terminal"
        };

        // Act
        var result = bookingService.BookSeats(bookingRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.BookingId);
        
        // Verify reservation was created in database
        var reservation = context.Reservations.FirstOrDefault(r => r.Id == result.BookingId);
        Assert.NotNull(reservation);
        Assert.Equal(bookingRequest.PassengerName, reservation.PassengerName);
        Assert.Equal(bookingRequest.PassengerEmail, reservation.PassengerEmail);
    }

    [Fact]
    public void BookSeats_WithInvalidSchedule_ShouldReturnFailure()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var bookingService = new BookingService(context);
        
        var bookingRequest = new BookSeatRequest
        {
            ScheduleId = 999, // Invalid schedule ID
            PassengerName = "John Doe",
            PassengerEmail = "john@example.com",
            PassengerPhone = "01712345678",
            SeatNumbers = new List<int> { 1, 2 },
            BoardingPoint = "Mohakhali",
            DroppingPoint = "Rajshahi Terminal"
        };

        // Act
        var result = bookingService.BookSeats(bookingRequest);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("Schedule not found", result.ErrorMessage);
    }
}