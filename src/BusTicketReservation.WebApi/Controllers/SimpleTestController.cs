using Microsoft.AspNetCore.Mvc;

namespace BusTicketReservation.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimpleTestController : ControllerBase
    {
        private readonly ILogger<SimpleTestController> _logger;

        public SimpleTestController(ILogger<SimpleTestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simple signup endpoint for testing
        /// </summary>
        [HttpPost("signup")]
        public async Task<ActionResult> Signup([FromBody] SimpleSignupRequest request)
        {
            try
            {
                _logger.LogInformation("Processing signup for email: {Email}", request.Email);

                // Generate a random OTP code
                var otpCode = new Random().Next(100000, 999999).ToString();
                
                _logger.LogInformation("Generated OTP {Code} for {Email}", otpCode, request.Email);

                return Ok(new
                {
                    Success = true,
                    Message = $"OTP sent to {request.Email}",
                    RequiresOtp = true,
                    OtpCode = otpCode, // In production, this would be sent via email
                    User = new
                    {
                        Email = request.Email,
                        FullName = request.FullName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup for email: {Email}", request.Email);
                return StatusCode(500, new { message = "Internal server error during signup" });
            }
        }

        /// <summary>
        /// Simple verify OTP endpoint for testing
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<ActionResult> VerifyOtp([FromBody] SimpleVerifyOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Verifying OTP for email: {Email}", request.Email);

                // For testing, accept any 6-digit code
                if (request.OtpCode?.Length == 6 && request.OtpCode.All(char.IsDigit))
                {
                    var userId = Guid.NewGuid();
                    
                    return Ok(new
                    {
                        Success = true,
                        Message = "OTP verified successfully",
                        Token = $"test-jwt-token-{userId}",
                        User = new
                        {
                            Id = userId,
                            Email = request.Email,
                            FullName = $"User {request.Email}",
                            IsEmailVerified = true
                        }
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid OTP code"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for email: {Email}", request.Email);
                return StatusCode(500, new { message = "Internal server error during OTP verification" });
            }
        }

        /// <summary>
        /// Health check endpoint for testing API connectivity
        /// </summary>
        [HttpGet("health")]
        public ActionResult<object> HealthCheck()
        {
            return Ok(new 
            { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                message = "Simple Test API is running successfully"
            });
        }
    }

    // Simple DTOs for testing
    public class SimpleSignupRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class SimpleVerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string Purpose { get; set; } = "SIGNUP";
    }
}