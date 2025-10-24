namespace BusTicketReservation.Application.DTOs;

public class BookSeatInputDto
{
    public Guid BusScheduleId { get; set; }
    public Guid SeatId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string BoardingPoint { get; set; } = string.Empty;
    public string DroppingPoint { get; set; } = string.Empty;
}

public class BookSeatResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? TicketId { get; set; }
    public Guid? PassengerId { get; set; }
    public TicketDto? Ticket { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class TicketDto
{
    public Guid TicketId { get; set; }
    public Guid SeatId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public Guid PassengerId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid BusScheduleId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime JourneyDate { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string BoardingPoint { get; set; } = string.Empty;
    public string DroppingPoint { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
}