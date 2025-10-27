using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Infrastructure.Data;

namespace BusTicketReservation.Infrastructure.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(BusTicketDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByPassengerIdAsync(Guid passengerId)
    {
        return await _dbSet
            .Include(t => t.Passenger)
            .Include(t => t.Seat)
            .ThenInclude(s => s.Bus)
            .Include(t => t.BusSchedule)
            .ThenInclude(bs => bs.Route)
            .Where(t => t.PassengerId == passengerId)
            .OrderByDescending(t => t.BookingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByBusScheduleIdAsync(Guid busScheduleId)
    {
        return await _dbSet
            .Include(t => t.Passenger)
            .Include(t => t.Seat)
            .Include(t => t.BusSchedule)
            .Where(t => t.BusScheduleId == busScheduleId)
            .OrderBy(t => t.Seat.SeatNumber)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketWithDetailsAsync(Guid ticketId)
    {
        return await _dbSet
            .Include(t => t.Passenger)
            .Include(t => t.Seat)
            .ThenInclude(s => s.Bus)
            .Include(t => t.BusSchedule)
            .ThenInclude(bs => bs.Route)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task<bool> HasActiveTicketForSeatAsync(Guid seatId)
    {
        var currentDate = DateTime.UtcNow;
        
        return await _dbSet
            .Include(t => t.BusSchedule)
            .AnyAsync(t => t.SeatId == seatId && 
                          t.BusSchedule.DepartureTime > currentDate);
    }
}