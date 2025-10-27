using BusTicketReservation.Application.Interfaces;

namespace BusTicketReservation.WebApi.Services
{
    public class NoOpUnitOfWork : IUnitOfWork
    {
        public IBusRepository Buses => throw new NotImplementedException("Bus repository not implemented for testing");
        public IRouteRepository Routes => throw new NotImplementedException("Route repository not implemented for testing");
        public IBusScheduleRepository BusSchedules => throw new NotImplementedException("BusSchedule repository not implemented for testing");
        public ISeatRepository Seats => throw new NotImplementedException("Seat repository not implemented for testing");
        public ITicketRepository Tickets => throw new NotImplementedException("Ticket repository not implemented for testing");
        public IPassengerRepository Passengers => throw new NotImplementedException("Passenger repository not implemented for testing");

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return 0; // No actual changes for Supabase implementation
        }

        public async Task BeginTransactionAsync()
        {
            // No transaction needed for testing
        }

        public async Task CommitTransactionAsync()
        {
            // No transaction needed for testing
        }

        public async Task RollbackTransactionAsync()
        {
            // No transaction needed for testing
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}