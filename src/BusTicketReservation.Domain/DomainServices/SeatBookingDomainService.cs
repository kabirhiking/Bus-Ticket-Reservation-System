using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Domain.Exceptions;

namespace BusTicketReservation.Domain.DomainServices;

public class SeatBookingDomainService
{
    public bool CanBookSeat(Seat seat, BusSchedule schedule)
    {
        if (seat == null)
            throw new ArgumentNullException(nameof(seat));
            
        if (schedule == null)
            throw new ArgumentNullException(nameof(schedule));
            
        // Check if seat belongs to the bus in the schedule
        if (seat.BusId != schedule.BusId)
            return false;
            
        // Check if seat is available
        if (seat.Status != SeatStatus.Available)
            return false;
            
        // Check if schedule is valid for booking
        if (!schedule.IsAvailableForBooking())
            return false;
            
        return true;
    }
    
    public Ticket BookSeat(Seat seat, Passenger passenger, BusSchedule schedule,
                          string boardingPoint, string droppingPoint)
    {
        if (!CanBookSeat(seat, schedule))
            throw new SeatNotAvailableException(
                seat?.SeatNumber ?? "Unknown", 
                seat?.Id ?? Guid.Empty,
                "Seat cannot be booked at this time");
                
        // Create the ticket first
        var ticket = new Ticket(
            seat.Id,
            passenger.Id,
            schedule.Id,
            boardingPoint,
            droppingPoint,
            schedule.Price);
            
        // Book the seat (this will raise domain events)
        seat.Book();
        
        return ticket;
    }
    
    public void CancelBooking(Ticket ticket, string cancellationReason)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));
            
        if (!ticket.CanBeCancelled())
            throw new InvalidBookingException("Ticket cannot be cancelled", ticket.Id);
            
        // Cancel the ticket
        ticket.Cancel(cancellationReason);
        
        // Release the seat if it exists
        if (ticket.Seat != null)
        {
            ticket.Seat.Release();
        }
    }
    
    public bool ValidateBookingRules(Seat seat, Passenger passenger, BusSchedule schedule)
    {
        // Additional business rules can be implemented here
        
        // Rule: Passenger cannot book multiple seats on the same schedule
        var existingTickets = passenger.Tickets
            .Where(t => t.BusScheduleId == schedule.Id && t.IsActive())
            .ToList();
            
        if (existingTickets.Any())
            return false;
            
        // Rule: Cannot book more than 24 hours before journey
        // (This is just an example rule - adjust based on requirements)
        var bookingDeadline = schedule.JourneyDate.AddHours(-24);
        if (DateTime.Now > bookingDeadline)
            return false;
            
        return true;
    }
}