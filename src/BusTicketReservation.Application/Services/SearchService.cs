using BusTicketReservation.Application.DTOs;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BusTicketReservation.Application.Services;

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IUnitOfWork unitOfWork, ILogger<SearchService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BusSearchResultDto> SearchAvailableBusesAsync(SearchBusRequestDto request)
    {
        _logger.LogInformation("Searching buses from {From} to {To} on {Date}", 
            request.From, request.To, request.JourneyDate);

        var availableBuses = await SearchAvailableBusesAsync(request.From, request.To, request.JourneyDate);

        return new BusSearchResultDto
        {
            AvailableBuses = availableBuses,
            TotalBuses = availableBuses.Count,
            SearchFrom = request.From,
            SearchTo = request.To,
            SearchDate = request.JourneyDate
        };
    }

    public async Task<List<AvailableBusDto>> SearchAvailableBusesAsync(string from, string to, DateTime journeyDate)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(from))
            throw new ArgumentException("From city cannot be empty", nameof(from));

        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("To city cannot be empty", nameof(to));

        if (journeyDate.Date < DateTime.Today)
            throw new ArgumentException("Journey date cannot be in the past", nameof(journeyDate));

        try
        {
            // Get available schedules from repository
            var schedules = await _unitOfWork.BusSchedules.GetAvailableSchedulesAsync(from, to, journeyDate);

            // Map to DTOs
            var availableBuses = schedules.Select(MapToAvailableBusDto).ToList();

            _logger.LogInformation("Found {Count} available buses for {From} to {To} on {Date}", 
                availableBuses.Count, from, to, journeyDate);

            return availableBuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching buses from {From} to {To} on {Date}", from, to, journeyDate);
            throw;
        }
    }

    private static AvailableBusDto MapToAvailableBusDto(BusScheduleDto schedule)
    {
        return new AvailableBusDto
        {
            BusScheduleId = schedule.ScheduleId,
            BusId = schedule.BusId,
            CompanyName = schedule.CompanyName,
            BusName = schedule.BusName,
            BusType = schedule.BusType,
            FromCity = schedule.FromCity,
            ToCity = schedule.ToCity,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            JourneyDate = schedule.JourneyDate,
            Price = schedule.Price,
            Currency = schedule.Currency,
            TotalSeats = schedule.TotalSeats,
            AvailableSeats = schedule.AvailableSeats,
            BookedSeats = schedule.TotalSeats - schedule.AvailableSeats,
            JourneyDuration = schedule.ArrivalTime - schedule.DepartureTime,
            Distance = schedule.Distance
        };
    }
}