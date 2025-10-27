using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Supabase;

namespace BusTicketReservation.WebApi.Services
{
    public class SupabaseUserRepository : IUserRepository
    {
        private readonly Client _supabase;
        private readonly ILogger<SupabaseUserRepository> _logger;
        private readonly IConfiguration _configuration;
        private static readonly Dictionary<string, User> _userStorage = new();

        public SupabaseUserRepository(Client supabase, ILogger<SupabaseUserRepository> logger, IConfiguration configuration)
        {
            _supabase = supabase;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("🔍 Searching for user in Supabase database: {Email}", email);
                
                // Try to query real Supabase database first
                try
                {
                    _logger.LogInformation("📊 Querying Supabase users table for: {Email}", email);
                    
                    // Real Supabase database query using HTTP client
                    using var httpClient = new HttpClient();
                    var supabaseUrl = _configuration["Supabase:Url"];
                    var apiKey = _configuration["Supabase:Key"];
                    
                    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    
                    var queryUrl = $"{supabaseUrl}/rest/v1/users?email=eq.{email}&select=*";
                    _logger.LogInformation("📊 Executing: GET {QueryUrl}", queryUrl);
                    
                    var response = await httpClient.GetAsync(queryUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("📊 Supabase response: {Response}", jsonContent);
                        
                        var users = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, System.Text.Json.JsonElement>>>(jsonContent);
                        if (users?.Count > 0)
                        {
                            var userData = users[0];
                            var dbUser = new User
                            {
                                Email = userData["email"].GetString()!,
                                FullName = userData.ContainsKey("full_name") && userData["full_name"].ValueKind != System.Text.Json.JsonValueKind.Null 
                                    ? userData["full_name"].GetString() 
                                    : null,
                                IsEmailVerified = userData["is_email_verified"].GetBoolean(),
                                IsActive = userData["is_active"].GetBoolean()
                            };
                            
                            // Handle optional fields
                            if (userData.ContainsKey("last_login_at") && userData["last_login_at"].ValueKind != System.Text.Json.JsonValueKind.Null)
                            {
                                dbUser.UpdateLastLogin(); // This will set LastLoginAt to current time
                            }
                            
                            _logger.LogInformation("✅ Found user in Supabase database: {Email}", email);
                            _userStorage[email] = dbUser; // Cache in memory
                            return dbUser;
                        }
                    }
                    
                    _logger.LogInformation("📊 No user found in Supabase database: {Email}", email);
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning("⚠️ Supabase database query failed, using memory cache: {Error}", dbEx.Message);
                }

                // Fallback to in-memory storage
                if (_userStorage.TryGetValue(email, out var user))
                {
                    _logger.LogInformation("✅ Found user in memory cache: {Email}", email);
                    return user;
                }

                _logger.LogInformation("❌ User not found anywhere: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error getting user: {Email}", email);
                return null;
            }
        }

        public async Task<User?> GetByIdAsync(Guid id) => _userStorage.Values.FirstOrDefault(u => u.Id == id);
        public async Task<User> AddAsync(User entity)
        {
            try
            {
                _logger.LogInformation("💾 Saving user to Supabase database: {Email}", entity.Email);
                
                // Try to save to real Supabase database first
                try
                {
                    var userData = new Dictionary<string, object>
                    {
                        ["id"] = entity.Id.ToString(),
                        ["email"] = entity.Email,
                        ["full_name"] = entity.FullName ?? "",
                        ["is_email_verified"] = entity.IsEmailVerified,
                        ["is_active"] = entity.IsActive,
                        ["created_at"] = entity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    };

                    _logger.LogInformation("📊 Supabase REST Insert: POST /rest/v1/users");
                    _logger.LogInformation("📊 User data to insert: {UserData}", System.Text.Json.JsonSerializer.Serialize(userData));

                    // Real Supabase database insert using HTTP client
                    using var httpClient = new HttpClient();
                    var supabaseUrl = _configuration["Supabase:Url"];
                    var apiKey = _configuration["Supabase:Key"];
                    
                    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
                    
                    var insertUrl = $"{supabaseUrl}/rest/v1/users";
                    var jsonPayload = System.Text.Json.JsonSerializer.Serialize(userData);
                    var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                    
                    _logger.LogInformation("📊 Executing: POST {InsertUrl}", insertUrl);
                    _logger.LogInformation("📊 Payload: {Payload}", jsonPayload);
                    
                    var response = await httpClient.PostAsync(insertUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("✅ User saved to Supabase database successfully: {Response}", responseContent);
                        _logger.LogInformation("🎯 Check your Supabase dashboard - user data should now be visible!");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("⚠️ Supabase insert failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning("⚠️ Supabase database insert failed, using memory storage: {Error}", dbEx.Message);
                    _logger.LogInformation("📊 Fallback: Storing user in memory cache: {Email}", entity.Email);
                }

                // Always store in memory as cache/fallback
                _userStorage[entity.Email] = entity;
                
                _logger.LogInformation("✅ User processing completed: {UserId}", entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error adding user: {Email}", entity.Email);
                throw;
            }
        }
        public async Task UpdateAsync(User entity)
        {
            try
            {
                _logger.LogInformation("🔄 Updating user in storage: {Email}", entity.Email);
                
                // Use the protected method to update timestamp
                entity.GetType().GetMethod("MarkAsUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(entity, null);
                _userStorage[entity.Email] = entity;
                
                // Log what would be updated in Supabase
                _logger.LogInformation("📊 Supabase UPDATE SQL: UPDATE users SET full_name='{FullName}', is_email_verified={IsEmailVerified}, is_active={IsActive}, updated_at='{UpdatedAt}', last_login_at='{LastLoginAt}' WHERE email='{Email}'", 
                    entity.FullName ?? "NULL", entity.IsEmailVerified.ToString().ToLower(), entity.IsActive.ToString().ToLower(), 
                    entity.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? "NULL", 
                    entity.LastLoginAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? "NULL", entity.Email);
                
                _logger.LogInformation("✅ User updated successfully: {UserId}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error updating user: {Email}", entity.Email);
                throw;
            }
        }
        public async Task DeleteAsync(User entity) { _userStorage.Remove(entity.Email); }
        public async Task<bool> ExistsAsync(Guid id) => _userStorage.Values.Any(u => u.Id == id);
        public async Task<IEnumerable<User>> GetAllAsync() => _userStorage.Values.ToList();
        public async Task<bool> EmailExistsAsync(string email) => _userStorage.ContainsKey(email);
        public async Task<User?> GetByIdWithTicketsAsync(Guid id) => await GetByIdAsync(id);
    }
}
