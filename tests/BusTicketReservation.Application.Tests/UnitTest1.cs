using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using BusTicketReservation.Application.Services;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Application.DTOs;

namespace BusTicketReservation.Application.Tests;

public class SearchServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IBusScheduleRepository> _mockBusScheduleRepository;
    private readonly Mock<ILogger<SearchService>> _mockLogger;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockBusScheduleRepository = new Mock<IBusScheduleRepository>();
        _mockLogger = new Mock<ILogger<SearchService>>();
        
        _mockUnitOfWork.Setup(x => x.BusSchedules).Returns(_mockBusScheduleRepository.Object);
        _searchService = new SearchService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SearchAvailableBusesAsync_WithValidRequest_ReturnsAvailableBuses()
    {
        // Arrange
        var request = new SearchBusRequestDto
        {
            From = "Dhaka",
            To = "Chittagong",
            JourneyDate = DateTime.Today.AddDays(1)
        };

        var mockSchedules = new List<BusScheduleDto>
        {
            new BusScheduleDto
            {
                ScheduleId = Guid.NewGuid(),
                BusId = Guid.NewGuid(),
                BusName = "Test Bus",
                CompanyName = "Test Company",
                BusType = "AC",
                FromCity = "Dhaka",
                ToCity = "Chittagong",
                DepartureTime = DateTime.Today.AddDays(1).AddHours(8),
                ArrivalTime = DateTime.Today.AddDays(1).AddHours(14),
                JourneyDate = request.JourneyDate,
                Price = 1200,
                Currency = "BDT",
                TotalSeats = 40,
                AvailableSeats = 25,
                Distance = 244
            }
        };

        _mockBusScheduleRepository.Setup(x => x.GetAvailableSchedulesAsync(
            request.From, request.To, request.JourneyDate))
            .ReturnsAsync(mockSchedules);

        // Act
        var result = await _searchService.SearchAvailableBusesAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.AvailableBuses);
        Assert.Equal(1, result.TotalBuses);
        Assert.Equal(request.From, result.SearchFrom);
        Assert.Equal(request.To, result.SearchTo);
        Assert.Equal(request.JourneyDate, result.SearchDate);
    }

    [Fact]
    public async Task SearchAvailableBusesAsync_WithEmptyFrom_ThrowsArgumentException()
    {
        // Arrange
        var request = new SearchBusRequestDto
        {
            From = "",
            To = "Chittagong",
            JourneyDate = DateTime.Today.AddDays(1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.SearchAvailableBusesAsync(request));
    }

    [Fact]
    public async Task SearchAvailableBusesAsync_WithPastDate_ThrowsArgumentException()
    {
        // Arrange
        var request = new SearchBusRequestDto
        {
            From = "Dhaka",
            To = "Chittagong",
            JourneyDate = DateTime.Today.AddDays(-1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _searchService.SearchAvailableBusesAsync(request));
    }
}
}
