using BusTicketReservation.Domain.Common;
using BusTicketReservation.Domain.ValueObjects;

namespace BusTicketReservation.Domain.Entities;

public class Route : BaseEntity
{
    public string FromCity { get; private set; }
    public string ToCity { get; private set; }
    public decimal Distance { get; private set; }
    public TimeSpan EstimatedDuration { get; private set; }
    
    // Navigation Properties
    private readonly List<BusSchedule> _busSchedules = new();
    public IReadOnlyList<BusSchedule> BusSchedules => _busSchedules.AsReadOnly();
    
    private Route() { } // For EF Core
    
    public Route(string fromCity, string toCity, decimal distance, TimeSpan estimatedDuration)
    {
        if (string.IsNullOrWhiteSpace(fromCity))
            throw new ArgumentNullException(nameof(fromCity));
            
        if (string.IsNullOrWhiteSpace(toCity))
            throw new ArgumentNullException(nameof(toCity));
            
        if (fromCity.Equals(toCity, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("From and To cities cannot be the same");
            
        if (distance <= 0)
            throw new ArgumentException("Distance must be positive", nameof(distance));
            
        if (estimatedDuration <= TimeSpan.Zero)
            throw new ArgumentException("Estimated duration must be positive", nameof(estimatedDuration));
            
        FromCity = fromCity.Trim();
        ToCity = toCity.Trim();
        Distance = distance;
        EstimatedDuration = estimatedDuration;
    }
    
    public RouteInfo GetRouteInfo() => new RouteInfo(FromCity, ToCity);
    
    public bool MatchesRoute(string from, string to)
    {
        return FromCity.Equals(from, StringComparison.OrdinalIgnoreCase) &&
               ToCity.Equals(to, StringComparison.OrdinalIgnoreCase);
    }
    
    public void AddBusSchedule(BusSchedule schedule)
    {
        if (schedule.RouteId != Id)
            throw new ArgumentException("Schedule route ID doesn't match this route");
            
        _busSchedules.Add(schedule);
        MarkAsUpdated();
    }
    
    public override string ToString() => $"{FromCity} → {ToCity} ({Distance}km)";
}