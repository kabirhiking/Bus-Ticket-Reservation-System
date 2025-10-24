using BusTicketReservation.Application.DTOs;

namespace BusTicketReservation.Application.Interfaces;

public interface ISearchService
{
    Task<BusSearchResultDto> SearchAvailableBusesAsync(SearchBusRequestDto request);
    Task<List<AvailableBusDto>> SearchAvailableBusesAsync(string from, string to, DateTime journeyDate);
}

public interface IBookingService
{
    Task<SeatPlanDto> GetSeatPlanAsync(Guid busScheduleId);
    Task<BookSeatResultDto> BookSeatAsync(BookSeatInputDto input);
    Task<BookSeatResultDto> CancelBookingAsync(Guid ticketId, string cancellationReason);
    Task<TicketDto?> GetTicketDetailsAsync(Guid ticketId);
}