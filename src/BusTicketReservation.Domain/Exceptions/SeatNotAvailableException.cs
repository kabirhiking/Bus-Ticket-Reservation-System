namespace BusTicketReservation.Domain.Exceptions;

public class SeatNotAvailableException : DomainException
{
    public string SeatNumber { get; }
    public Guid SeatId { get; }
    
    public SeatNotAvailableException(string seatNumber, Guid seatId, string message) 
        : base(message)
    {
        SeatNumber = seatNumber;
        SeatId = seatId;
    }
    
    public SeatNotAvailableException(string message) : base(message)
    {
        SeatNumber = string.Empty;
        SeatId = Guid.Empty;
    }
}