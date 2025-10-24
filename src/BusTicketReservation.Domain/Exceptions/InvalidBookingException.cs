namespace BusTicketReservation.Domain.Exceptions;

public class InvalidBookingException : DomainException
{
    public Guid? TicketId { get; }
    public Guid? SeatId { get; }
    
    public InvalidBookingException(string message) : base(message)
    {
    }
    
    public InvalidBookingException(string message, Guid ticketId) : base(message)
    {
        TicketId = ticketId;
    }
    
    public InvalidBookingException(string message, Guid ticketId, Guid seatId) : base(message)
    {
        TicketId = ticketId;
        SeatId = seatId;
    }
}