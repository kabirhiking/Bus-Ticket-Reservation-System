using BusTicketReservation.Domain.Common;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Domain.DomainEvents;

namespace BusTicketReservation.Domain.Entities;

public enum TicketStatus
{
    Confirmed,
    Cancelled,
    Used
}

public class Ticket : BaseEntity, IAggregateRoot
{
    public Guid SeatId { get; private set; }
    public Guid PassengerId { get; private set; }
    public Guid BusScheduleId { get; private set; }
    public string BoardingPoint { get; private set; }
    public string DroppingPoint { get; private set; }
    public DateTime BookingDate { get; private set; }
    public Money Price { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    
    // Navigation Properties
    public virtual Seat Seat { get; private set; }
    public virtual Passenger Passenger { get; private set; }
    public virtual BusSchedule BusSchedule { get; private set; }
    
    private Ticket() { } // For EF Core
    
    public Ticket(Guid seatId, Guid passengerId, Guid busScheduleId,
                  string boardingPoint, string droppingPoint, Money price)
    {
        if (seatId == Guid.Empty)
            throw new ArgumentException("Seat ID cannot be empty", nameof(seatId));
            
        if (passengerId == Guid.Empty)
            throw new ArgumentException("Passenger ID cannot be empty", nameof(passengerId));
            
        if (busScheduleId == Guid.Empty)
            throw new ArgumentException("Bus schedule ID cannot be empty", nameof(busScheduleId));
            
        if (string.IsNullOrWhiteSpace(boardingPoint))
            throw new ArgumentNullException(nameof(boardingPoint));
            
        if (string.IsNullOrWhiteSpace(droppingPoint))
            throw new ArgumentNullException(nameof(droppingPoint));
            
        if (price == null)
            throw new ArgumentNullException(nameof(price));
            
        SeatId = seatId;
        PassengerId = passengerId;
        BusScheduleId = busScheduleId;
        BoardingPoint = boardingPoint.Trim();
        DroppingPoint = droppingPoint.Trim();
        BookingDate = DateTime.UtcNow;
        Price = price;
        Status = TicketStatus.Confirmed;
        
        AddDomainEvent(new TicketBookedEvent(Id, SeatId, PassengerId, BusScheduleId, Price.Amount));
    }
    
    public void Cancel(string reason)
    {
        if (Status == TicketStatus.Cancelled)
            throw new InvalidOperationException("Ticket is already cancelled");
            
        if (Status == TicketStatus.Used)
            throw new InvalidOperationException("Cannot cancel a used ticket");
            
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));
            
        Status = TicketStatus.Cancelled;
        CancellationReason = reason.Trim();
        MarkAsUpdated();
        
        AddDomainEvent(new TicketCancelledEvent(Id, SeatId, BusScheduleId, reason));
    }
    
    public void MarkAsUsed()
    {
        if (Status == TicketStatus.Cancelled)
            throw new InvalidOperationException("Cannot use a cancelled ticket");
            
        if (Status == TicketStatus.Used)
            return; // Already used
            
        Status = TicketStatus.Used;
        MarkAsUpdated();
    }
    
    public bool IsActive() => Status == TicketStatus.Confirmed;
    
    public bool CanBeCancelled() => Status == TicketStatus.Confirmed && BusSchedule?.JourneyDate >= DateTime.Today;
    
    public override string ToString() => $"Ticket {Id} - {BoardingPoint} to {DroppingPoint} ({Status})";
}