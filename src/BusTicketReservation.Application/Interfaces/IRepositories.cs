using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Application.Interfaces;

public interface ISeatRepository : IEntityRepository<Seat>
{
    Task<IEnumerable<Seat>> GetSeatsByBusIdAsync(Guid busId);
    Task<Seat?> GetAvailableSeatAsync(Guid seatId);
    Task<IEnumerable<Seat>> GetAvailableSeatsByBusIdAsync(Guid busId);
    Task<Seat?> GetSeatWithDetailsAsync(Guid seatId);
}

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetTicketsByPassengerIdAsync(Guid passengerId);
    Task<IEnumerable<Ticket>> GetTicketsByBusScheduleIdAsync(Guid busScheduleId);
    Task<Ticket?> GetTicketWithDetailsAsync(Guid ticketId);
    Task<bool> HasActiveTicketForSeatAsync(Guid seatId);
}

public interface IPassengerRepository : IEntityRepository<Passenger>
{
    Task<Passenger?> GetByMobileNumberAsync(string mobileNumber);
    Task<Passenger?> GetPassengerWithTicketsAsync(Guid passengerId);
}