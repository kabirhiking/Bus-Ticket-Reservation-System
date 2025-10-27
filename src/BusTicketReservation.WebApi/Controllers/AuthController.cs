using BusTicketReservation.Application.DTOs;
using BusTicketReservation.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace BusTicketReservation.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data.",
                        RequiresOtp = false
                    });
                }

                var result = await _authService.LoginAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", request.Email);
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = "An internal server error occurred. Please try again later.",
                    RequiresOtp = false
                });
            }
        }

        [HttpPost("signup")]
        public async Task<ActionResult<SignupResponseDto>> Signup([FromBody] SignupRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new SignupResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data.",
                        RequiresOtp = false
                    });
                }

                var result = await _authService.SignupAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during signup for email: {Email}", request.Email);
                return StatusCode(500, new SignupResponseDto
                {
                    Success = false,
                    Message = "An internal server error occurred. Please try again later.",
                    RequiresOtp = false
                });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<VerifyOtpResponseDto>> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data."
                    });
                }

                var result = await _authService.VerifyOtpAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OTP verification for email: {Email}", request.Email);
                return StatusCode(500, new VerifyOtpResponseDto
                {
                    Success = false,
                    Message = "An internal server error occurred. Please try again later."
                });
            }
        }

        [HttpPost("resend-otp")]
        public async Task<ActionResult<ResendOtpResponseDto>> ResendOtp([FromBody] ResendOtpRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResendOtpResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data."
                    });
                }

                var result = await _authService.ResendOtpAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OTP resend for email: {Email}", request.Email);
                return StatusCode(500, new ResendOtpResponseDto
                {
                    Success = false,
                    Message = "An internal server error occurred. Please try again later."
                });
            }
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid token."
                    });
                }

                var result = await _authService.RefreshTokenAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "An internal server error occurred. Please try again later."
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            try
            {
                // Try multiple claim types to find the user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                               ?? User.FindFirst("sub")?.Value
                               ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Unable to extract user ID from token claims");
                    return Unauthorized(new { success = false, message = "Invalid token." });
                }

                _logger.LogInformation("Logging out user: {UserId}", userId);
                await _authService.LogoutAsync(userId);
                
                return Ok(new { success = true, message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return StatusCode(500, new { success = false, message = "An internal server error occurred." });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                // Try multiple claim types to find the user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                               ?? User.FindFirst("sub")?.Value
                               ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Unable to extract user ID from token claims for profile");
                    return Unauthorized(new { success = false, message = "Invalid token." });
                }

                _logger.LogInformation("Fetching profile for user: {UserId}", userId);
                var result = await _authService.GetUserProfileAsync(userId);
                
                if (result.IsSuccess)
                {
                    return Ok(result.Value);
                }
                
                return NotFound(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user profile");
                return StatusCode(500, new { success = false, message = "An internal server error occurred." });
            }
        }
    }
}