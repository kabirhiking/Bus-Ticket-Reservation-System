namespace BusTicketReservation.Application.DTOs;

public class SearchBusRequestDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public DateTime JourneyDate { get; set; }
}

public class BusSearchResultDto
{
    public List<AvailableBusDto> AvailableBuses { get; set; } = new();
    public int TotalBuses { get; set; }
    public string SearchFrom { get; set; } = string.Empty;
    public string SearchTo { get; set; } = string.Empty;
    public DateTime SearchDate { get; set; }
}