using BusTicketReservation.Domain.Common;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Domain.DomainEvents;

namespace BusTicketReservation.Domain.Entities;

public class Seat : BaseEntity
{
    public Guid BusId { get; private set; }
    public string SeatNumber { get; private set; }
    public string Row { get; private set; }
    public SeatStatus Status { get; private set; }
    
    // Navigation Properties
    public virtual Bus Bus { get; private set; }
    public virtual Ticket? Ticket { get; private set; }
    
    private Seat() { } // For EF Core
    
    public Seat(Guid busId, string seatNumber, string row)
    {
        if (busId == Guid.Empty)
            throw new ArgumentException("Bus ID cannot be empty", nameof(busId));
            
        if (string.IsNullOrWhiteSpace(seatNumber))
            throw new ArgumentNullException(nameof(seatNumber));
            
        if (string.IsNullOrWhiteSpace(row))
            throw new ArgumentNullException(nameof(row));
            
        BusId = busId;
        SeatNumber = seatNumber.Trim();
        Row = row.Trim();
        Status = SeatStatus.Available;
    }
    
    public void Book()
    {
        if (Status != SeatStatus.Available)
            throw new InvalidOperationException($"Seat {SeatNumber} is not available for booking. Current status: {Status}");
            
        Status = SeatStatus.Booked;
        MarkAsUpdated();
        AddDomainEvent(new SeatBookedEvent(Id, BusId, SeatNumber));
    }
    
    public void MarkAsSold()
    {
        if (Status != SeatStatus.Booked)
            throw new InvalidOperationException($"Only booked seats can be marked as sold. Current status: {Status}");
            
        Status = SeatStatus.Sold;
        MarkAsUpdated();
    }
    
    public void Release()
    {
        if (Status == SeatStatus.Available)
            return; // Already available
            
        Status = SeatStatus.Available;
        MarkAsUpdated();
        AddDomainEvent(new SeatReleasedEvent(Id, BusId, SeatNumber));
    }
    
    public bool IsAvailable() => Status == SeatStatus.Available;
    
    public bool IsBooked() => Status == SeatStatus.Booked;
    
    public bool IsSold() => Status == SeatStatus.Sold;
    
    public override string ToString() => $"Seat {SeatNumber} ({Row}) - {Status}";
}