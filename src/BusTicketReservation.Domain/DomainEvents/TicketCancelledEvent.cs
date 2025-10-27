using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.DomainEvents;

public class TicketCancelledEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid TicketId { get; }
    public Guid SeatId { get; }
    public Guid BusScheduleId { get; }
    public string CancellationReason { get; }
    
    public TicketCancelledEvent(Guid ticketId, Guid seatId, Guid busScheduleId, string cancellationReason)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TicketId = ticketId;
        SeatId = seatId;
        BusScheduleId = busScheduleId;
        CancellationReason = cancellationReason;
    }
}