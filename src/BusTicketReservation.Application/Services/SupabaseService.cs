using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using Supabase;

namespace BusTicketReservation.Application.Services
{
    public class SupabaseService : ISupabaseService
    {
        private readonly Client _supabaseClient;

        public SupabaseService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public Client GetClient()
        {
            return _supabaseClient;
        }

        // User-specific methods
        public async Task<SupabaseUser?> GetUserByEmailAsync(string email)
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseUser>()
                    .Where(x => x.Email == email)
                    .Single();
                
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SupabaseUser?> GetUserByIdAsync(Guid id)
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseUser>()
                    .Where(x => x.Id == id)
                    .Single();
                
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SupabaseUser?> CreateUserAsync(SupabaseUser user)
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseUser>()
                    .Insert(user);
                
                return response?.Model;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SupabaseUser?> UpdateUserAsync(SupabaseUser user)
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseUser>()
                    .Where(x => x.Id == user.Id)
                    .Update(user);
                
                return response?.Model;
            }
            catch
            {
                return null;
            }
        }

        // OTP-specific methods
        public async Task<SupabaseOtpCode?> GetOtpCodeAsync(string email, string code)
        {
            try
            {
                // First get user by email
                var user = await GetUserByEmailAsync(email);
                if (user == null) return null;

                var response = await _supabaseClient
                    .From<SupabaseOtpCode>()
                    .Where(x => x.UserId == user.Id && x.Code == code && !x.IsUsed)
                    .Single();
                
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SupabaseOtpCode?> CreateOtpCodeAsync(SupabaseOtpCode otpCode)
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseOtpCode>()
                    .Insert(otpCode);
                
                return response?.Model;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SupabaseOtpCode?> UpdateOtpCodeAsync(SupabaseOtpCode otpCode)
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseOtpCode>()
                    .Where(x => x.Id == otpCode.Id)
                    .Update(otpCode);
                
                return response?.Model;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteExpiredOtpCodesAsync(Guid userId)
        {
            try
            {
                await _supabaseClient
                    .From<SupabaseOtpCode>()
                    .Where(x => x.UserId == userId && x.ExpiresAt < DateTime.UtcNow)
                    .Delete();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Test-specific methods
        public async Task<List<SupabaseTest>> GetAllTestsAsync()
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseTest>()
                    .Get();
                
                return response?.Models ?? new List<SupabaseTest>();
            }
            catch
            {
                return new List<SupabaseTest>();
            }
        }

        public async Task<SupabaseTest?> GetTestByIdAsync(long id)
        {
            try
            {
                var response = await _supabaseClient
                    .From<SupabaseTest>()
                    .Where(x => x.Id == id)
                    .Single();
                
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SupabaseTest?> CreateTestAsync()
        {
            try
            {
                // Don't set Id or CreatedAt as they are auto-generated by the database
                var testRecord = new SupabaseTest();
                
                var response = await _supabaseClient
                    .From<SupabaseTest>()
                    .Insert(testRecord);
                
                return response?.Model;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error creating test record: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        // Generic methods (simplified)
        public async Task<T?> GetAsync<T>(string table, string id) where T : class, new()
        {
            // This is a placeholder - specific methods above should be used
            return null;
        }

        public async Task<List<T>> GetAllAsync<T>(string table) where T : class, new()
        {
            // This is a placeholder - specific methods above should be used
            return new List<T>();
        }

        public async Task<T?> CreateAsync<T>(string table, T entity) where T : class, new()
        {
            // This is a placeholder - specific methods above should be used
            return null;
        }

        public async Task<T?> UpdateAsync<T>(string table, string id, T entity) where T : class, new()
        {
            // This is a placeholder - specific methods above should be used
            return null;
        }

        public async Task<bool> DeleteAsync(string table, string id)
        {
            // This is a placeholder - specific methods above should be used
            return false;
        }
    }
}