namespace BusTicketReservation.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBusRepository Buses { get; }
    IRouteRepository Routes { get; }
    IBusScheduleRepository BusSchedules { get; }
    ISeatRepository Seats { get; }
    ITicketRepository Tickets { get; }
    IPassengerRepository Passengers { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}