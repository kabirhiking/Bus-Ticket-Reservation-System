namespace BusTicketReservation.Application.DTOs;

public class SeatDto
{
    public Guid SeatId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsBooked { get; set; }
    public bool IsSold { get; set; }
}

public class SeatPlanDto
{
    public Guid BusScheduleId { get; set; }
    public Guid BusId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public List<SeatDto> Seats { get; set; } = new();
    public DateTime JourneyDate { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
}