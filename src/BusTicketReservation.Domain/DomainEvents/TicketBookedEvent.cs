using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.DomainEvents;

public class TicketBookedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid TicketId { get; }
    public Guid SeatId { get; }
    public Guid PassengerId { get; }
    public Guid BusScheduleId { get; }
    public decimal Amount { get; }
    
    public TicketBookedEvent(Guid ticketId, Guid seatId, Guid passengerId, Guid busScheduleId, decimal amount)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TicketId = ticketId;
        SeatId = seatId;
        PassengerId = passengerId;
        BusScheduleId = busScheduleId;
        Amount = amount;
    }
}