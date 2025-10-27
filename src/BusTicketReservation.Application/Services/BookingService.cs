using BusTicketReservation.Application.DTOs;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.DomainServices;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BusTicketReservation.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SeatBookingDomainService _seatBookingDomainService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IUnitOfWork unitOfWork, 
        SeatBookingDomainService seatBookingDomainService,
        ILogger<BookingService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _seatBookingDomainService = seatBookingDomainService ?? throw new ArgumentNullException(nameof(seatBookingDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SeatPlanDto> GetSeatPlanAsync(Guid busScheduleId)
    {
        _logger.LogInformation("Getting seat plan for schedule {ScheduleId}", busScheduleId);

        var schedule = await _unitOfWork.BusSchedules.GetScheduleWithDetailsAsync(busScheduleId);
        if (schedule == null)
            throw new ArgumentException($"Bus schedule with ID {busScheduleId} not found");

        var bus = await _unitOfWork.Buses.GetBusWithSeatsAsync(schedule.BusId);
        if (bus == null)
            throw new ArgumentException($"Bus with ID {schedule.BusId} not found");

        var seats = await _unitOfWork.Seats.GetSeatsByBusIdAsync(bus.Id);

        var seatPlan = new SeatPlanDto
        {
            BusScheduleId = schedule.Id,
            BusId = bus.Id,
            BusName = bus.Name,
            CompanyName = bus.CompanyName,
            TotalSeats = bus.TotalSeats,
            AvailableSeats = bus.GetAvailableSeatsCount(),
            JourneyDate = schedule.JourneyDate,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            FromCity = schedule.Route?.FromCity ?? string.Empty,
            ToCity = schedule.Route?.ToCity ?? string.Empty,
            Seats = seats.Select(MapToSeatDto).ToList()
        };

        _logger.LogInformation("Retrieved seat plan with {SeatCount} seats for schedule {ScheduleId}", 
            seatPlan.Seats.Count, busScheduleId);

        return seatPlan;
    }

    public async Task<BookSeatResultDto> BookSeatAsync(BookSeatInputDto input)
    {
        _logger.LogInformation("Booking seat {SeatId} for passenger {PassengerName}", 
            input.SeatId, input.PassengerName);

        var errors = ValidateBookingInput(input);
        if (errors.Any())
        {
            return new BookSeatResultDto
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            };
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get required entities
            var schedule = await _unitOfWork.BusSchedules.GetScheduleWithDetailsAsync(input.BusScheduleId);
            if (schedule == null)
            {
                return CreateFailureResult("Bus schedule not found");
            }

            var seat = await _unitOfWork.Seats.GetSeatWithDetailsAsync(input.SeatId);
            if (seat == null)
            {
                return CreateFailureResult("Seat not found");
            }

            // Check if seat belongs to the correct bus
            if (seat.BusId != schedule.BusId)
            {
                return CreateFailureResult("Seat does not belong to the selected bus");
            }

            // Get or create passenger
            var passenger = await GetOrCreatePassengerAsync(input);

            // Validate booking rules
            if (!_seatBookingDomainService.CanBookSeat(seat, schedule))
            {
                return CreateFailureResult("Seat is not available for booking");
            }

            if (!_seatBookingDomainService.ValidateBookingRules(seat, passenger, schedule))
            {
                return CreateFailureResult("Booking validation failed. You may already have a booking on this schedule.");
            }

            // Create the booking
            var ticket = _seatBookingDomainService.BookSeat(
                seat, passenger, schedule, input.BoardingPoint, input.DroppingPoint);

            // Save changes
            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully booked seat {SeatNumber} for passenger {PassengerName}. Ticket ID: {TicketId}", 
                seat.SeatNumber, passenger.Name, ticket.Id);

            return new BookSeatResultDto
            {
                Success = true,
                Message = "Seat booked successfully",
                TicketId = ticket.Id,
                PassengerId = passenger.Id,
                Ticket = MapToTicketDto(ticket, seat, passenger, schedule)
            };
        }
        catch (SeatNotAvailableException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogWarning(ex, "Seat booking failed - seat not available");
            return CreateFailureResult(ex.Message);
        }
        catch (InvalidBookingException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogWarning(ex, "Seat booking failed - invalid booking");
            return CreateFailureResult(ex.Message);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error booking seat {SeatId} for passenger {PassengerName}", 
                input.SeatId, input.PassengerName);
            return CreateFailureResult("An error occurred while booking the seat. Please try again.");
        }
    }

    public async Task<BookSeatResultDto> CancelBookingAsync(Guid ticketId, string cancellationReason)
    {
        _logger.LogInformation("Cancelling booking for ticket {TicketId}", ticketId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var ticket = await _unitOfWork.Tickets.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null)
            {
                return CreateFailureResult("Ticket not found");
            }

            _seatBookingDomainService.CancelBooking(ticket, cancellationReason);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully cancelled ticket {TicketId}", ticketId);

            return new BookSeatResultDto
            {
                Success = true,
                Message = "Booking cancelled successfully"
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);
            return CreateFailureResult("An error occurred while cancelling the booking. Please try again.");
        }
    }

    public async Task<TicketDto?> GetTicketDetailsAsync(Guid ticketId)
    {
        var ticket = await _unitOfWork.Tickets.GetTicketWithDetailsAsync(ticketId);
        if (ticket == null)
            return null;

        return MapToTicketDto(ticket, ticket.Seat, ticket.Passenger, ticket.BusSchedule);
    }

    private async Task<Passenger> GetOrCreatePassengerAsync(BookSeatInputDto input)
    {
        var existingPassenger = await _unitOfWork.Passengers.GetByMobileNumberAsync(input.MobileNumber);
        if (existingPassenger != null)
        {
            // Update email if provided
            if (!string.IsNullOrWhiteSpace(input.Email) && existingPassenger.Email != input.Email)
            {
                existingPassenger.UpdateContactInfo(input.Email);
                await _unitOfWork.Passengers.UpdateAsync(existingPassenger);
            }
            return existingPassenger;
        }

        // Create new passenger
        var newPassenger = new Passenger(input.PassengerName, input.MobileNumber, input.Email);
        return await _unitOfWork.Passengers.AddAsync(newPassenger);
    }

    private static List<string> ValidateBookingInput(BookSeatInputDto input)
    {
        var errors = new List<string>();

        if (input.BusScheduleId == Guid.Empty)
            errors.Add("Bus schedule ID is required");

        if (input.SeatId == Guid.Empty)
            errors.Add("Seat ID is required");

        if (string.IsNullOrWhiteSpace(input.PassengerName))
            errors.Add("Passenger name is required");

        if (string.IsNullOrWhiteSpace(input.MobileNumber))
            errors.Add("Mobile number is required");

        if (string.IsNullOrWhiteSpace(input.BoardingPoint))
            errors.Add("Boarding point is required");

        if (string.IsNullOrWhiteSpace(input.DroppingPoint))
            errors.Add("Dropping point is required");

        return errors;
    }

    private static BookSeatResultDto CreateFailureResult(string message)
    {
        return new BookSeatResultDto
        {
            Success = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }

    private static SeatDto MapToSeatDto(Seat seat)
    {
        return new SeatDto
        {
            SeatId = seat.Id,
            SeatNumber = seat.SeatNumber,
            Row = seat.Row,
            Status = seat.Status.Value,
            IsAvailable = seat.IsAvailable(),
            IsBooked = seat.IsBooked(),
            IsSold = seat.IsSold()
        };
    }

    private static TicketDto MapToTicketDto(Ticket ticket, Seat seat, Passenger passenger, BusSchedule schedule)
    {
        return new TicketDto
        {
            TicketId = ticket.Id,
            SeatId = seat.Id,
            SeatNumber = seat.SeatNumber,
            PassengerId = passenger.Id,
            PassengerName = passenger.Name,
            MobileNumber = passenger.MobileNumber,
            Email = passenger.Email,
            BusScheduleId = schedule.Id,
            BusName = schedule.Bus?.Name ?? string.Empty,
            CompanyName = schedule.Bus?.CompanyName ?? string.Empty,
            FromCity = schedule.Route?.FromCity ?? string.Empty,
            ToCity = schedule.Route?.ToCity ?? string.Empty,
            JourneyDate = schedule.JourneyDate,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            BoardingPoint = ticket.BoardingPoint,
            DroppingPoint = ticket.DroppingPoint,
            BookingDate = ticket.BookingDate,
            Price = ticket.Price.Amount,
            Currency = ticket.Price.Currency,
            Status = ticket.Status.ToString()
        };
    }
}