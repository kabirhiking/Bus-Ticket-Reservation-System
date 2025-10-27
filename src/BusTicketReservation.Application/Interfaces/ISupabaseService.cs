using BusTicketReservation.Domain.Entities;
using Supabase;

namespace BusTicketReservation.Application.Interfaces
{
    public interface ISupabaseService
    {
        Client GetClient();
        
        // User-specific methods
        Task<SupabaseUser?> GetUserByEmailAsync(string email);
        Task<SupabaseUser?> GetUserByIdAsync(Guid id);
        Task<SupabaseUser?> CreateUserAsync(SupabaseUser user);
        Task<SupabaseUser?> UpdateUserAsync(SupabaseUser user);
        
        // OTP-specific methods
        Task<SupabaseOtpCode?> GetOtpCodeAsync(string email, string code);
        Task<SupabaseOtpCode?> CreateOtpCodeAsync(SupabaseOtpCode otpCode);
        Task<SupabaseOtpCode?> UpdateOtpCodeAsync(SupabaseOtpCode otpCode);
        Task<bool> DeleteExpiredOtpCodesAsync(Guid userId);
        
        // Test-specific methods
        Task<List<SupabaseTest>> GetAllTestsAsync();
        Task<SupabaseTest?> GetTestByIdAsync(long id);
        Task<SupabaseTest?> CreateTestAsync();
        
        // Generic methods (for future use)
        Task<T?> GetAsync<T>(string table, string id) where T : class, new();
        Task<List<T>> GetAllAsync<T>(string table) where T : class, new();
        Task<T?> CreateAsync<T>(string table, T entity) where T : class, new();
        Task<T?> UpdateAsync<T>(string table, string id, T entity) where T : class, new();
        Task<bool> DeleteAsync(string table, string id);
    }
}