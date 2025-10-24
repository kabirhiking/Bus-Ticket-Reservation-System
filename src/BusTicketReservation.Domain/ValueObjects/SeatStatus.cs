using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.ValueObjects;

public class SeatStatus : ValueObject
{
    public string Value { get; private set; }
    
    public static readonly SeatStatus Available = new("Available");
    public static readonly SeatStatus Booked = new("Booked");
    public static readonly SeatStatus Sold = new("Sold");
    
    private SeatStatus(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public static SeatStatus FromString(string status)
    {
        return status?.ToLower() switch
        {
            "available" => Available,
            "booked" => Booked,
            "sold" => Sold,
            _ => throw new ArgumentException($"Invalid seat status: {status}")
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(SeatStatus status) => status.Value;
}