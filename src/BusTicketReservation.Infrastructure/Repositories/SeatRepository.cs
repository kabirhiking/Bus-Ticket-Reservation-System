using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Infrastructure.Data;

namespace BusTicketReservation.Infrastructure.Repositories;

public class SeatRepository : EntityRepository<Seat>, ISeatRepository
{
    public SeatRepository(BusTicketDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Seat>> GetSeatsByBusIdAsync(Guid busId)
    {
        return await _dbSet
            .Where(s => s.BusId == busId)
            .OrderBy(s => s.SeatNumber)
            .ToListAsync();
    }

    public async Task<Seat?> GetAvailableSeatAsync(Guid seatId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Id == seatId && s.Status == SeatStatus.Available);
    }

    public async Task<IEnumerable<Seat>> GetAvailableSeatsByBusIdAsync(Guid busId)
    {
        return await _dbSet
            .Where(s => s.BusId == busId && s.Status == SeatStatus.Available)
            .OrderBy(s => s.SeatNumber)
            .ToListAsync();
    }

    public async Task<Seat?> GetSeatWithDetailsAsync(Guid seatId)
    {
        return await _dbSet
            .Include(s => s.Bus)
            .FirstOrDefaultAsync(s => s.Id == seatId);
    }
}