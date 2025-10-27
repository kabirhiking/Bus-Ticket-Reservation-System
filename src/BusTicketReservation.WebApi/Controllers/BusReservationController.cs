using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Application.DTOs;
using BusTicketReservation.WebApi.DTOs.Requests;
using BusTicketReservation.WebApi.DTOs.Responses;
using BusTicketReservation.WebApi.Services;

namespace BusTicketReservation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BusReservationController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly IBookingService _bookingService;
    private readonly IMappingService _mappingService;
    private readonly IValidator<SearchBusesRequest> _searchValidator;
    private readonly IValidator<BookTicketRequest> _bookValidator;
    private readonly IValidator<CancelTicketRequest> _cancelValidator;
    private readonly ILogger<BusReservationController> _logger;

    public BusReservationController(
        ISearchService searchService,
        IBookingService bookingService,
        IMappingService mappingService,
        IValidator<SearchBusesRequest> searchValidator,
        IValidator<BookTicketRequest> bookValidator,
        IValidator<CancelTicketRequest> cancelValidator,
        ILogger<BusReservationController> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
        _searchValidator = searchValidator ?? throw new ArgumentNullException(nameof(searchValidator));
        _bookValidator = bookValidator ?? throw new ArgumentNullException(nameof(bookValidator));
        _cancelValidator = cancelValidator ?? throw new ArgumentNullException(nameof(cancelValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Search for available buses between two cities on a specific date
    /// </summary>
    /// <param name="request">Search criteria including from/to cities, date, and passenger count</param>
    /// <returns>List of available bus schedules</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchBusesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SearchBusesResponse>>> SearchBuses([FromBody] SearchBusesRequest request)
    {
        try
        {
            _logger.LogInformation("Searching buses from {FromCity} to {ToCity} on {JourneyDate}", 
                request.FromCity, request.ToCity, request.JourneyDate);

            // Validate request
            var validationResult = await _searchValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<SearchBusesResponse>.ErrorResult(errors));
            }

            // Search for buses
            var schedules = await _searchService.SearchAvailableBusesAsync(
                request.FromCity, 
                request.ToCity, 
                request.JourneyDate);

            var response = _mappingService.MapToSearchResponse(schedules, request);
            
            _logger.LogInformation("Found {Count} available buses", response.SearchResultCount);
            
            return Ok(ApiResponse<SearchBusesResponse>.SuccessResult(response, 
                $"Found {response.SearchResultCount} available buses"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching buses");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<SearchBusesResponse>.ErrorResult("An error occurred while searching buses"));
        }
    }

    /// <summary>
    /// Get seat availability for a specific bus schedule
    /// </summary>
    /// <param name="scheduleId">The schedule ID to check seat availability</param>
    /// <returns>Seat availability information</returns>
    [HttpGet("schedule/{scheduleId}/seats")]
    [ProducesResponseType(typeof(ApiResponse<SeatAvailabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SeatAvailabilityResponse>>> GetSeatAvailability(Guid scheduleId)
    {
        try
        {
            _logger.LogInformation("Getting seat availability for schedule {ScheduleId}", scheduleId);

            var seatPlan = await _bookingService.GetSeatPlanAsync(scheduleId);
            if (seatPlan == null)
            {
                return NotFound(ApiResponse<SeatAvailabilityResponse>.ErrorResult("Bus schedule not found"));
            }

            var response = _mappingService.MapToSeatAvailability(seatPlan);
            
            return Ok(ApiResponse<SeatAvailabilityResponse>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting seat availability");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<SeatAvailabilityResponse>.ErrorResult("An error occurred while getting seat availability"));
        }
    }

    /// <summary>
    /// Book tickets for selected seats
    /// </summary>
    /// <param name="request">Booking request with schedule, seats, and passenger information</param>
    /// <returns>Booking confirmation with ticket details</returns>
    [HttpPost("book")]
    [ProducesResponseType(typeof(ApiResponse<BookingConfirmationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<BookingConfirmationResponse>>> BookTickets([FromBody] BookTicketRequest request)
    {
        try
        {
            _logger.LogInformation("Booking tickets for schedule {ScheduleId}, seats: {SeatNumbers}", 
                request.ScheduleId, string.Join(", ", request.SeatNumbers));

            // Validate request
            var validationResult = await _bookValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<BookingConfirmationResponse>.ErrorResult(errors));
            }

            // Get seat plan to find seat IDs by seat numbers
            var seatPlan = await _bookingService.GetSeatPlanAsync(request.ScheduleId);
            if (seatPlan == null)
            {
                return BadRequest(ApiResponse<BookingConfirmationResponse>.ErrorResult("Bus schedule not found"));
            }

            var ticketResults = new List<TicketDto>();
            
            // Book each seat individually
            foreach (var seatNumber in request.SeatNumbers)
            {
                var seat = seatPlan.Seats.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat == null)
                {
                    return BadRequest(ApiResponse<BookingConfirmationResponse>.ErrorResult($"Seat {seatNumber} not found"));
                }

                if (!seat.IsAvailable)
                {
                    return Conflict(ApiResponse<BookingConfirmationResponse>.ErrorResult($"Seat {seatNumber} is not available"));
                }

                var bookSeatInput = new BookSeatInputDto
                {
                    BusScheduleId = request.ScheduleId,
                    SeatId = seat.SeatId,
                    PassengerName = request.PassengerInfo.Name,
                    MobileNumber = request.PassengerInfo.MobileNumber,
                    Email = request.PassengerInfo.Email,
                    BoardingPoint = seatPlan.FromCity, // Default to from city
                    DroppingPoint = seatPlan.ToCity     // Default to to city
                };

                var bookingResult = await _bookingService.BookSeatAsync(bookSeatInput);
                if (!bookingResult.Success)
                {
                    return Conflict(ApiResponse<BookingConfirmationResponse>.ErrorResult(bookingResult.Message));
                }

                if (bookingResult.Ticket != null)
                {
                    ticketResults.Add(bookingResult.Ticket);
                }
            }

            var response = _mappingService.MapToBookingConfirmation(ticketResults);
            
            _logger.LogInformation("Successfully booked {TicketCount} tickets", response.Tickets.Count);
            
            return Ok(ApiResponse<BookingConfirmationResponse>.SuccessResult(response, "Tickets booked successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Booking conflict occurred");
            return Conflict(ApiResponse<BookingConfirmationResponse>.ErrorResult(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid booking request");
            return BadRequest(ApiResponse<BookingConfirmationResponse>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while booking tickets");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<BookingConfirmationResponse>.ErrorResult("An error occurred while booking tickets"));
        }
    }

    /// <summary>
    /// Get ticket details by ticket ID
    /// </summary>
    /// <param name="ticketId">The ticket ID</param>
    /// <returns>Ticket details</returns>
    [HttpGet("ticket/{ticketId}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TicketDto>>> GetTicketDetails(Guid ticketId)
    {
        try
        {
            _logger.LogInformation("Getting ticket details for ID {TicketId}", ticketId);

            var ticket = await _bookingService.GetTicketDetailsAsync(ticketId);
            if (ticket == null)
            {
                return NotFound(ApiResponse<TicketDto>.ErrorResult("Ticket not found"));
            }
            
            return Ok(ApiResponse<TicketDto>.SuccessResult(ticket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting ticket details");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TicketDto>.ErrorResult("An error occurred while getting ticket details"));
        }
    }

    /// <summary>
    /// Cancel a ticket booking
    /// </summary>
    /// <param name="request">Cancellation request with ticket ID and reason</param>
    /// <returns>Cancellation confirmation</returns>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(ApiResponse<BookSeatResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<BookSeatResultDto>>> CancelTicket([FromBody] CancelTicketRequest request)
    {
        try
        {
            _logger.LogInformation("Cancelling ticket {TicketId}", request.TicketId);

            // Validate request
            var validationResult = await _cancelValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<BookSeatResultDto>.ErrorResult(errors));
            }

            var result = await _bookingService.CancelBookingAsync(request.TicketId, request.Reason);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<BookSeatResultDto>.ErrorResult(result.Message));
            }
            
            return Ok(ApiResponse<BookSeatResultDto>.SuccessResult(result, "Ticket cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling ticket");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<BookSeatResultDto>.ErrorResult("An error occurred while cancelling the ticket"));
        }
    }
}