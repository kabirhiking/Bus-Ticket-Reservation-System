using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Infrastructure.Data;

namespace BusTicketReservation.Infrastructure.Repositories;

public class BusScheduleRepository : Repository<BusSchedule>, IBusScheduleRepository
{
    public BusScheduleRepository(BusTicketDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BusScheduleDto>> GetAvailableSchedulesAsync(string from, string to, DateTime journeyDate)
    {
        var startDate = journeyDate.Date;
        var endDate = startDate.AddDays(1);

        var schedules = await _dbSet
            .Include(s => s.Bus)
            .ThenInclude(b => b.Seats)
            .Include(s => s.Route)
            .Where(s => s.Route.FromCity.ToLower() == from.ToLower() &&
                       s.Route.ToCity.ToLower() == to.ToLower() &&
                       s.DepartureTime >= startDate &&
                       s.DepartureTime < endDate)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync();

        return schedules.Select(s => new BusScheduleDto
        {
            ScheduleId = s.Id,
            BusId = s.BusId,
            BusName = s.Bus?.Name ?? "Unknown",
            CompanyName = s.Bus?.CompanyName ?? "Unknown",
            BusType = s.Bus?.BusType ?? "Unknown",
            RouteId = s.RouteId,
            FromCity = s.Route?.FromCity ?? "Unknown",
            ToCity = s.Route?.ToCity ?? "Unknown",
            DepartureTime = s.DepartureTime,
            ArrivalTime = s.ArrivalTime,
            JourneyDate = journeyDate,
            Price = s.Price.Amount,
            Currency = s.Price.Currency,
            TotalSeats = s.Bus?.TotalSeats ?? 0,
            AvailableSeats = s.Bus?.Seats?.Count(seat => seat.Status == SeatStatus.Available) ?? 0,
            Distance = s.Route?.Distance ?? 0
        });
    }

    public async Task<BusSchedule?> GetScheduleWithDetailsAsync(Guid scheduleId)
    {
        return await _dbSet
            .Include(s => s.Bus)
            .ThenInclude(b => b.Seats)
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == scheduleId);
    }

    public async Task<IEnumerable<BusSchedule>> GetSchedulesByBusIdAsync(Guid busId)
    {
        return await _dbSet
            .Include(s => s.Bus)
            .Include(s => s.Route)
            .Where(s => s.BusId == busId)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<BusSchedule>> GetSchedulesByRouteIdAsync(Guid routeId)
    {
        return await _dbSet
            .Include(s => s.Bus)
            .Include(s => s.Route)
            .Where(s => s.RouteId == routeId)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<BusSchedule>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Include(s => s.Bus)
            .Include(s => s.Route)
            .Where(s => s.DepartureTime >= fromDate && s.DepartureTime <= toDate)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<BusSchedule>> GetByRouteAndDateAsync(Guid routeId, DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        return await _dbSet
            .Include(s => s.Bus)
            .Include(s => s.Route)
            .Where(s => s.RouteId == routeId &&
                       s.DepartureTime >= startDate &&
                       s.DepartureTime < endDate)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync();
    }

    public async Task<BusSchedule?> GetByBusAndDepartureTimeAsync(Guid busId, DateTime departureTime)
    {
        return await _dbSet
            .Include(s => s.Bus)
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.BusId == busId && s.DepartureTime == departureTime);
    }

    public async Task<IEnumerable<BusSchedule>> GetUpcomingSchedulesAsync(DateTime fromDateTime)
    {
        return await _dbSet
            .Include(s => s.Bus)
            .Include(s => s.Route)
            .Where(s => s.DepartureTime >= fromDateTime)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<BusSchedule>> GetActiveSchedulesAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(s => s.Bus)
            .Include(s => s.Route)
            .Where(s => s.DepartureTime > now)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync();
    }
}