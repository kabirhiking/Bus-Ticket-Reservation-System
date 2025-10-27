using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Infrastructure.Data;
using BusTicketReservation.Infrastructure.Repositories;

namespace BusTicketReservation.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly BusTicketDbContext _context;
    private bool _disposed = false;

    public UnitOfWork(BusTicketDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        
        // Initialize repositories
        Buses = new BusRepository(_context);
        Routes = new RouteRepository(_context);
        BusSchedules = new BusScheduleRepository(_context);
        Seats = new SeatRepository(_context);
        Tickets = new TicketRepository(_context);
        Passengers = new PassengerRepository(_context);
    }

    public IBusRepository Buses { get; private set; }
    public IRouteRepository Routes { get; private set; }
    public IBusScheduleRepository BusSchedules { get; private set; }
    public ISeatRepository Seats { get; private set; }
    public ITicketRepository Tickets { get; private set; }
    public IPassengerRepository Passengers { get; private set; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        if (_context.Database.CurrentTransaction == null)
        {
            await _context.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionAsync()
    {
        var transaction = _context.Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.CommitAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        var transaction = _context.Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.RollbackAsync();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
        
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}