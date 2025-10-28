using BusTicketReservation.Domain.Common;
using BusTicketReservation.Domain.ValueObjects;

namespace BusTicketReservation.Domain.Entities;

public class BusSchedule : BaseEntity, IAggregateRoot
{
    public Guid BusId { get; private set; }
    public Guid RouteId { get; private set; }
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public DateTime JourneyDate { get; private set; }
    public Money Price { get; private set; }
    
    // Navigation Properties
    public virtual Bus Bus { get; private set; }
    public virtual Route Route { get; private set; }
    private readonly List<Ticket> _tickets = new();
    public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();
    
    private BusSchedule() { } // For EF Core
    
    public BusSchedule(Guid busId, Guid routeId, DateTime departureTime, 
                      DateTime arrivalTime, DateTime journeyDate, Money price)
    {
        if (busId == Guid.Empty)
            throw new ArgumentException("Bus ID cannot be empty", nameof(busId));
            
        if (routeId == Guid.Empty)
            throw new ArgumentException("Route ID cannot be empty", nameof(routeId));
            
        if (departureTime >= arrivalTime)
            throw new ArgumentException("Departure time must be before arrival time");
            
        if (journeyDate.Date < DateTime.Today)
            throw new ArgumentException("Journey date cannot be in the past");
            
        if (price == null)
            throw new ArgumentNullException(nameof(price));
            
        BusId = busId;
        RouteId = routeId;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        JourneyDate = journeyDate.Date;
        Price = price;
    }
    
    public TimeSpan GetJourneyDuration() => ArrivalTime - DepartureTime;
    
    public bool IsAvailableForBooking() => JourneyDate >= DateTime.Today;
    
    public int GetAvailableSeatsCount()
    {
        if (Bus == null) return 0;
        return Bus.GetAvailableSeatsCount();
    }
    
    public bool MatchesSearchCriteria(string from, string to, DateTime journeyDate)
    {
        return Route?.MatchesRoute(from, to) == true && 
               JourneyDate.Date == journeyDate.Date;
    }
    
    public void AddTicket(Ticket ticket)
    {
        if (ticket.BusScheduleId != Id)
            throw new ArgumentException("Ticket schedule ID doesn't match this schedule");
            
        _tickets.Add(ticket);
        MarkAsUpdated();
    }
}