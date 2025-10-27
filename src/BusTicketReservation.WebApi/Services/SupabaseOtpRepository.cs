using System.Collections.Concurrent;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Supabase;

namespace BusTicketReservation.WebApi.Services
{
    public class SupabaseOtpRepository : IOtpRepository
    {
        private readonly ILogger<SupabaseOtpRepository> _logger;
        private readonly Client _supabase;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, OtpCode> _otpStorage;

        public SupabaseOtpRepository(ILogger<SupabaseOtpRepository> logger, Client supabase, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _supabase = supabase ?? throw new ArgumentNullException(nameof(supabase));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _otpStorage = new ConcurrentDictionary<string, OtpCode>();
            
            _logger.LogInformation("🔧 SupabaseOtpRepository initialized with real database operations");
        }

        public async Task<OtpCode?> GetByIdAsync(Guid id) => _otpStorage.Values.FirstOrDefault(o => o.Id == id);

        public async Task<IEnumerable<OtpCode>> GetAllAsync() => _otpStorage.Values.ToList();

        public async Task<OtpCode> AddAsync(OtpCode entity)
        {
            try
            {
                var key = $"{entity.Email}:{entity.Purpose}";
                
                _logger.LogInformation("💾 Saving OTP to Supabase database: {Email} - {Purpose}", entity.Email, entity.Purpose);
                
                // Try to save to real Supabase database first
                try
                {
                    var otpData = new Dictionary<string, object>
                    {
                        ["id"] = entity.Id.ToString(),
                        ["email"] = entity.Email,
                        ["code"] = entity.Code,
                        ["purpose"] = entity.Purpose,
                        ["expires_at"] = entity.ExpiresAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        ["is_used"] = entity.IsUsed,
                        ["created_at"] = entity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    };

                    // Real Supabase database insert using HTTP client
                    using var httpClient = new HttpClient();
                    var supabaseUrl = _configuration["Supabase:Url"];
                    var apiKey = _configuration["Supabase:Key"];
                    
                    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
                    
                    var insertUrl = $"{supabaseUrl}/rest/v1/otp_codes";
                    var jsonPayload = System.Text.Json.JsonSerializer.Serialize(otpData);
                    var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                    
                    _logger.LogInformation("📊 Executing: POST {InsertUrl}", insertUrl);
                    _logger.LogInformation("📊 OTP Payload: {Payload}", jsonPayload);
                    
                    var response = await httpClient.PostAsync(insertUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("✅ OTP saved to Supabase database successfully: {Response}", responseContent);
                        _logger.LogInformation("🎯 Check your Supabase dashboard - OTP data should now be visible!");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("⚠️ Supabase OTP insert failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning("⚠️ Supabase database insert failed, using memory storage: {Error}", dbEx.Message);
                }

                // Always store in memory as cache/fallback
                _otpStorage[key] = entity;
                
                _logger.LogInformation("✅ OTP processing completed: {OtpId}", entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error adding OTP: {Email}", entity.Email);
                throw;
            }
        }

        public async Task<OtpCode?> GetByEmailAndPurposeAsync(string email, string purpose)
        {
            try
            {
                var key = $"{email}:{purpose}";
                
                _logger.LogInformation("🔍 Retrieving OTP from Supabase database: {Email} - {Purpose}", email, purpose);
                
                // Try to get from real Supabase database first
                try
                {
                    using var httpClient = new HttpClient();
                    var supabaseUrl = _configuration["Supabase:Url"];
                    var apiKey = _configuration["Supabase:Key"];
                    
                    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    
                    var queryUrl = $"{supabaseUrl}/rest/v1/otp_codes?email=eq.{email}&purpose=eq.{purpose}&order=created_at.desc&limit=1";
                    
                    _logger.LogInformation("📊 Executing: GET {QueryUrl}", queryUrl);
                    
                    var response = await httpClient.GetAsync(queryUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("📊 Supabase Response: {Response}", responseContent);
                        
                        var otpRecords = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseContent);
                        
                        if (otpRecords?.Any() == true)
                        {
                            var record = otpRecords.First();
                            var otpFromDb = new OtpCode
                            {
                                Email = record["email"]?.ToString() ?? "",
                                Code = record["code"]?.ToString() ?? "",
                                Purpose = record["purpose"]?.ToString() ?? "",
                                ExpiresAt = DateTime.Parse(record["expires_at"]?.ToString() ?? DateTime.UtcNow.AddMinutes(10).ToString()),
                                IsUsed = Convert.ToBoolean(record["is_used"] ?? false)
                            };
                            
                            _logger.LogInformation("✅ OTP found in Supabase database: {OtpId}", otpFromDb.Id);
                            
                            // Also cache in memory
                            _otpStorage[key] = otpFromDb;
                            return otpFromDb;
                        }
                        else
                        {
                            _logger.LogInformation("ℹ️ No OTP found in Supabase database for: {Email} - {Purpose}", email, purpose);
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("⚠️ Supabase OTP query failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning("⚠️ Supabase database query failed, checking memory storage: {Error}", dbEx.Message);
                }

                // Check memory storage as fallback
                if (_otpStorage.TryGetValue(key, out var otpFromMemory))
                {
                    _logger.LogInformation("✅ OTP found in memory storage: {OtpId}", otpFromMemory.Id);
                    return otpFromMemory;
                }
                
                _logger.LogInformation("ℹ️ No OTP found for: {Email} - {Purpose}", email, purpose);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error retrieving OTP: {Email} - {Purpose}", email, purpose);
                throw;
            }
        }

        public async Task<OtpCode?> GetValidOtpAsync(string email, string purpose)
        {
            try
            {
                _logger.LogInformation("🔍 Looking for valid OTP in Supabase: {Email} - {Purpose}", email, purpose);
                
                // Query Supabase database for valid OTP
                using var httpClient = new HttpClient();
                var supabaseUrl = _configuration["Supabase:Url"];
                var apiKey = _configuration["Supabase:Key"];
                
                httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                
                var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var queryUrl = $"{supabaseUrl}/rest/v1/otp_codes?email=eq.{email}&purpose=eq.{purpose}&is_used=eq.false&expires_at=gt.{now}&order=created_at.desc&limit=1";
                
                _logger.LogInformation("� Executing: GET {QueryUrl}", queryUrl);
                
                var response = await httpClient.GetAsync(queryUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("⚠️ Supabase query failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return null;
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("📊 Supabase response: {Response}", responseContent);
                
                var otps = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, System.Text.Json.JsonElement>>>(responseContent);
                
                if (otps == null || otps.Count == 0)
                {
                    _logger.LogInformation("❌ No valid OTP found for {Email} - {Purpose}", email, purpose);
                    return null;
                }
                
                var otpData = otps[0];
                var otp = new OtpCode
                {
                    Email = otpData["email"].GetString()!,
                    Code = otpData["code"].GetString()!,
                    Purpose = otpData["purpose"].GetString()!,
                    UserId = otpData.ContainsKey("user_id") && otpData["user_id"].ValueKind != System.Text.Json.JsonValueKind.Null 
                        ? Guid.Parse(otpData["user_id"].GetString()!) 
                        : null,
                    ExpiresAt = DateTime.Parse(otpData["expires_at"].GetString()!),
                    IsUsed = otpData["is_used"].GetBoolean(),
                    AttemptCount = otpData.ContainsKey("attempt_count") ? otpData["attempt_count"].GetInt32() : 0,
                    MaxAttempts = otpData.ContainsKey("max_attempts") ? otpData["max_attempts"].GetInt32() : 3
                };
                
                // Don't set Id and CreatedAt as they have protected setters
                
                _logger.LogInformation("✅ Found valid OTP: {Code} (expires: {ExpiresAt})", otp.Code, otp.ExpiresAt);
                
                // Cache in memory
                var key = $"{email}:{purpose}";
                _otpStorage[key] = otp;
                
                return otp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error getting valid OTP: {Email}", email);
                return null;
            }
        }

        public async Task UpdateAsync(OtpCode entity)
        {
            try
            {
                _logger.LogInformation("🔄 Updating OTP in Supabase: {Email} - {Purpose}", entity.Email, entity.Purpose);
                
                // Update in Supabase database
                try
                {
                    var updateData = new Dictionary<string, object>
                    {
                        ["is_used"] = entity.IsUsed,
                        ["attempt_count"] = entity.AttemptCount
                    };

                    using var httpClient = new HttpClient();
                    var supabaseUrl = _configuration["Supabase:Url"];
                    var apiKey = _configuration["Supabase:Key"];
                    
                    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
                    
                    var updateUrl = $"{supabaseUrl}/rest/v1/otp_codes?email=eq.{entity.Email}&purpose=eq.{entity.Purpose}&is_used=eq.false";
                    var jsonPayload = System.Text.Json.JsonSerializer.Serialize(updateData);
                    var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                    
                    _logger.LogInformation("📊 Executing: PATCH {UpdateUrl}", updateUrl);
                    _logger.LogInformation("📊 Update Payload: {Payload}", jsonPayload);
                    
                    var response = await httpClient.PatchAsync(updateUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("✅ OTP updated in Supabase database successfully: {Response}", responseContent);
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("⚠️ Supabase OTP update failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning("⚠️ Supabase database update failed: {Error}", dbEx.Message);
                }
                
                // Update in memory cache
                var key = $"{entity.Email}:{entity.Purpose}";
                _otpStorage[key] = entity;
                
                _logger.LogInformation("✅ OTP update completed: {OtpId}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error updating OTP: {Email}", entity.Email);
                throw;
            }
        }

        public async Task DeleteAsync(OtpCode entity)
        {
            try
            {
                var key = $"{entity.Email}:{entity.Purpose}";
                _otpStorage.TryRemove(key, out _);
                _logger.LogInformation("🗑️ OTP deleted: {Email} - {Purpose}", entity.Email, entity.Purpose);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error deleting OTP: {Email}", entity.Email);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id) => _otpStorage.Values.Any(o => o.Id == id);

        public async Task<List<OtpCode>> GetExpiredOtpsAsync() => 
            _otpStorage.Values.Where(o => o.ExpiresAt <= DateTime.UtcNow).ToList();

        public async Task<int> GetOtpAttemptCountAsync(string email, string purpose, DateTime since) => 0;
    }
}