namespace BusTicketReservation.WebApi.DTOs.Requests;

public class SearchBusesRequest
{
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime JourneyDate { get; set; }
    public int PassengerCount { get; set; } = 1;
}

public class BookTicketRequest
{
    public Guid ScheduleId { get; set; }
    public List<string> SeatNumbers { get; set; } = new();
    public PassengerInfoRequest PassengerInfo { get; set; } = new();
}

public class PassengerInfoRequest
{
    public string Name { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class CancelTicketRequest
{
    public Guid TicketId { get; set; }
    public string Reason { get; set; } = string.Empty;
}