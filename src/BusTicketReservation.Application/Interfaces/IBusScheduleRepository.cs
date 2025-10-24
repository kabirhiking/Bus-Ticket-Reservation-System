using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Application.Interfaces;

public interface IBusScheduleRepository : IEntityRepository<BusSchedule>
{
    Task<IEnumerable<BusScheduleDto>> GetAvailableSchedulesAsync(string from, string to, DateTime journeyDate);
    Task<BusSchedule?> GetScheduleWithDetailsAsync(Guid scheduleId);
    Task<IEnumerable<BusSchedule>> GetSchedulesByBusIdAsync(Guid busId);
    Task<IEnumerable<BusSchedule>> GetSchedulesByRouteIdAsync(Guid routeId);
}

public class BusScheduleDto
{
    public Guid ScheduleId { get; set; }
    public Guid BusId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public Guid RouteId { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime JourneyDate { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Distance { get; set; }
}