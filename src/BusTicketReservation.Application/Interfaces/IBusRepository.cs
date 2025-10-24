using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Application.Interfaces;

public interface IBusRepository : IRepository<Bus>
{
    Task<IEnumerable<Bus>> GetBusesByRouteAndDateAsync(string from, string to, DateTime journeyDate);
    Task<Bus?> GetBusWithSeatsAsync(Guid busId);
    Task<Bus?> GetBusWithSchedulesAsync(Guid busId);
}