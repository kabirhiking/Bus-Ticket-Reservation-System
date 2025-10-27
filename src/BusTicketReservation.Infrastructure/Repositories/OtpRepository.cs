using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusTicketReservation.Infrastructure.Repositories
{
    public class OtpRepository : EntityRepository<OtpCode>, IOtpRepository
    {
        public OtpRepository(BusTicketDbContext context) : base(context)
        {
        }

        public async Task<OtpCode?> GetValidOtpAsync(string email, string purpose)
        {
            return await _context.OtpCodes
                .Where(o => o.Email.ToLower() == email.ToLower() 
                         && o.Purpose == purpose 
                         && !o.IsUsed 
                         && o.ExpiresAt > DateTime.UtcNow
                         && o.AttemptCount < o.MaxAttempts)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<OtpCode>> GetExpiredOtpsAsync()
        {
            return await _context.OtpCodes
                .Where(o => o.ExpiresAt <= DateTime.UtcNow || o.IsUsed)
                .ToListAsync();
        }

        public async Task<int> GetOtpAttemptCountAsync(string email, string purpose, DateTime since)
        {
            return await _context.OtpCodes
                .Where(o => o.Email.ToLower() == email.ToLower() 
                         && o.Purpose == purpose 
                         && o.CreatedAt >= since)
                .SumAsync(o => o.AttemptCount);
        }
    }
}