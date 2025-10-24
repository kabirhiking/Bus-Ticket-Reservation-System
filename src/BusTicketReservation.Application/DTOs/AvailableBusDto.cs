namespace BusTicketReservation.Application.DTOs;

public class AvailableBusDto
{
    public Guid BusScheduleId { get; set; }
    public Guid BusId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string BusName { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime JourneyDate { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int BookedSeats { get; set; }
    public TimeSpan JourneyDuration { get; set; }
    public decimal Distance { get; set; }
}