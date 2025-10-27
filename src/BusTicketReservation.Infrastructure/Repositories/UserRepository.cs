using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusTicketReservation.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(BusTicketDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByIdWithTicketsAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Tickets)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}