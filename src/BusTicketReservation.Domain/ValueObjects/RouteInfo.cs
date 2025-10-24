using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.ValueObjects;

public class RouteInfo : ValueObject
{
    public string From { get; private set; }
    public string To { get; private set; }
    
    private RouteInfo() { } // For EF Core
    
    public RouteInfo(string from, string to)
    {
        if (string.IsNullOrWhiteSpace(from))
            throw new ArgumentNullException(nameof(from));
            
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentNullException(nameof(to));
            
        if (from.Equals(to, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("From and To cities cannot be the same");
            
        From = from.Trim();
        To = to.Trim();
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return From.ToLowerInvariant();
        yield return To.ToLowerInvariant();
    }
    
    public RouteInfo Reverse() => new RouteInfo(To, From);
    
    public override string ToString() => $"{From} → {To}";
}