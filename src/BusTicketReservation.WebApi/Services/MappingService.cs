using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Application.DTOs;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.WebApi.DTOs.Requests;
using BusTicketReservation.WebApi.DTOs.Responses;

namespace BusTicketReservation.WebApi.Services;

public interface IMappingService
{
    SearchBusesResponse MapToSearchResponse(IEnumerable<AvailableBusDto> schedules, SearchBusesRequest request);
    BookingConfirmationResponse MapToBookingConfirmation(IEnumerable<TicketDto> tickets);
    SeatAvailabilityResponse MapToSeatAvailability(SeatPlanDto seatPlan);
    BusScheduleInfo MapToBusScheduleInfo(AvailableBusDto schedule);
    TicketInfo MapToTicketInfo(TicketDto ticket);
}

public class MappingService : IMappingService
{
    public SearchBusesResponse MapToSearchResponse(IEnumerable<AvailableBusDto> schedules, SearchBusesRequest request)
    {
        var scheduleList = schedules.ToList();
        
        return new SearchBusesResponse
        {
            AvailableBuses = scheduleList.Select(MapToBusScheduleInfo).ToList(),
            FromCity = request.FromCity,
            ToCity = request.ToCity,
            JourneyDate = request.JourneyDate,
            SearchResultCount = scheduleList.Count
        };
    }

    public BookingConfirmationResponse MapToBookingConfirmation(IEnumerable<TicketDto> tickets)
    {
        var ticketList = tickets.ToList();
        var firstTicket = ticketList.FirstOrDefault();
        
        if (firstTicket == null)
            throw new InvalidOperationException("No tickets provided for booking confirmation");

        return new BookingConfirmationResponse
        {
            Tickets = ticketList.Select(MapToTicketInfo).ToList(),
            TotalAmount = ticketList.Sum(t => t.Price),
            Currency = firstTicket.Currency,
            BookingDate = firstTicket.BookingDate,
            BookingReference = firstTicket.TicketId.ToString(), // Use first ticket ID as reference
            PassengerDetails = new PassengerInfo
            {
                PassengerId = firstTicket.PassengerId,
                Name = firstTicket.PassengerName,
                MobileNumber = firstTicket.MobileNumber,
                Email = firstTicket.Email
            },
            JourneyDetails = new BusJourneyInfo
            {
                BusName = firstTicket.BusName,
                CompanyName = firstTicket.CompanyName,
                FromCity = firstTicket.FromCity,
                ToCity = firstTicket.ToCity,
                DepartureTime = firstTicket.DepartureTime,
                ArrivalTime = firstTicket.ArrivalTime,
                Distance = 0 // Distance not available in TicketDto, would need to be added
            }
        };
    }

    public SeatAvailabilityResponse MapToSeatAvailability(SeatPlanDto seatPlan)
    {
        return new SeatAvailabilityResponse
        {
            BusId = seatPlan.BusId,
            BusName = seatPlan.BusName,
            TotalSeats = seatPlan.TotalSeats,
            AvailableSeats = seatPlan.AvailableSeats,
            Seats = seatPlan.Seats.Select(s => new SeatInfo
            {
                SeatId = s.SeatId,
                SeatNumber = s.SeatNumber,
                Status = s.Status,
                IsAvailable = s.IsAvailable
            }).ToList()
        };
    }

    public BusScheduleInfo MapToBusScheduleInfo(AvailableBusDto schedule)
    {
        var duration = schedule.JourneyDuration;
        
        return new BusScheduleInfo
        {
            ScheduleId = schedule.BusScheduleId,
            BusId = schedule.BusId,
            BusName = schedule.BusName,
            CompanyName = schedule.CompanyName,
            BusType = schedule.BusType,
            FromCity = schedule.FromCity,
            ToCity = schedule.ToCity,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            Price = schedule.Price,
            Currency = schedule.Currency,
            TotalSeats = schedule.TotalSeats,
            AvailableSeats = schedule.AvailableSeats,
            Distance = schedule.Distance,
            Duration = $"{duration.Hours}h {duration.Minutes}m"
        };
    }

    public TicketInfo MapToTicketInfo(TicketDto ticket)
    {
        return new TicketInfo
        {
            TicketId = ticket.TicketId,
            TicketNumber = ticket.TicketId.ToString(), // Use TicketId as number since TicketNumber doesn't exist
            SeatNumber = ticket.SeatNumber,
            Price = ticket.Price,
            Status = ticket.Status
        };
    }
}