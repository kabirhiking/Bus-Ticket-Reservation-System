using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Infrastructure.Data;

namespace BusTicketReservation.Infrastructure.Repositories;

public class PassengerRepository : EntityRepository<Passenger>, IPassengerRepository
{
    public PassengerRepository(BusTicketDbContext context) : base(context)
    {
    }

    public async Task<Passenger?> GetByMobileNumberAsync(string mobileNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.MobileNumber == mobileNumber);
    }

    public async Task<Passenger?> GetPassengerWithTicketsAsync(Guid passengerId)
    {
        return await _dbSet
            .Include(p => p.Tickets)
            .ThenInclude(t => t.Seat)
            .ThenInclude(s => s.Bus)
            .Include(p => p.Tickets)
            .ThenInclude(t => t.BusSchedule)
            .ThenInclude(bs => bs.Route)
            .FirstOrDefaultAsync(p => p.Id == passengerId);
    }
}