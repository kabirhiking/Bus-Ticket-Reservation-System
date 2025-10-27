using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Infrastructure.Data;

namespace BusTicketReservation.Infrastructure.Repositories;

public class BusRepository : Repository<Bus>, IBusRepository
{
    public BusRepository(BusTicketDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Bus>> GetBusesByRouteAndDateAsync(string from, string to, DateTime journeyDate)
    {
        var startDate = journeyDate.Date;
        var endDate = startDate.AddDays(1);

        return await _dbSet
            .Include(b => b.Schedules)
            .ThenInclude(s => s.Route)
            .Include(b => b.Seats)
            .Where(b => b.Schedules.Any(s => 
                s.Route.FromCity.ToLower() == from.ToLower() &&
                s.Route.ToCity.ToLower() == to.ToLower() &&
                s.DepartureTime >= startDate &&
                s.DepartureTime < endDate))
            .ToListAsync();
    }

    public async Task<Bus?> GetBusWithSeatsAsync(Guid busId)
    {
        return await _dbSet
            .Include(b => b.Seats)
            .FirstOrDefaultAsync(b => b.Id == busId);
    }

    public async Task<Bus?> GetBusWithSchedulesAsync(Guid busId)
    {
        return await _dbSet
            .Include(b => b.Schedules)
            .ThenInclude(s => s.Route)
            .FirstOrDefaultAsync(b => b.Id == busId);
    }
}