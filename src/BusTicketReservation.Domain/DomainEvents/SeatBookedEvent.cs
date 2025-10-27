using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.DomainEvents;

public class SeatBookedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid SeatId { get; }
    public Guid BusId { get; }
    public string SeatNumber { get; }
    
    public SeatBookedEvent(Guid seatId, Guid busId, string seatNumber)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeatId = seatId;
        BusId = busId;
        SeatNumber = seatNumber;
    }
}