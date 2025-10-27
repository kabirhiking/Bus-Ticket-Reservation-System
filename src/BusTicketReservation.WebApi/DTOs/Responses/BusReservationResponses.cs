namespace BusTicketReservation.WebApi.DTOs.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ErrorResult(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
    }
}

public class SearchBusesResponse
{
    public List<BusScheduleInfo> AvailableBuses { get; set; } = new();
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime JourneyDate { get; set; }
    public int SearchResultCount { get; set; }
}

public class BusScheduleInfo
{
    public Guid ScheduleId { get; set; }
    public Guid BusId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Distance { get; set; }
    public string Duration { get; set; } = string.Empty;
}

public class BookingConfirmationResponse
{
    public List<TicketInfo> Tickets { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public PassengerInfo PassengerDetails { get; set; } = new();
    public BusJourneyInfo JourneyDetails { get; set; } = new();
}

public class TicketInfo
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PassengerInfo
{
    public Guid PassengerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class BusJourneyInfo
{
    public string BusName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Distance { get; set; }
}

public class SeatAvailabilityResponse
{
    public Guid BusId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public List<SeatInfo> Seats { get; set; } = new();
}

public class SeatInfo
{
    public Guid SeatId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}