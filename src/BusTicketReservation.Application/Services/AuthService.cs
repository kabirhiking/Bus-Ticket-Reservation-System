using BusTicketReservation.Application.DTOs;
using BusTicketReservation.Application.Common;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusTicketReservation.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IUserRepository userRepository,
            IOtpService otpService,
            IEmailService emailService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _otpService = otpService;
            _emailService = emailService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                // Check if user exists
                var user = await _userRepository.GetByEmailAsync(request.Email);
                
                if (user == null)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "User not found. Please sign up first.",
                        RequiresOtp = false
                    };
                }

                if (!user.IsActive)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Account is deactivated. Please contact support.",
                        RequiresOtp = false
                    };
                }

                // Generate and send OTP
                var otpCode = await _otpService.GenerateOtpAsync(request.Email, "LOGIN", user.Id);
                var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode.Code, "LOGIN");

                if (!emailSent)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Failed to send OTP. Please try again.",
                        RequiresOtp = false
                    };
                }

                await _unitOfWork.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "OTP sent to your email successfully.",
                    RequiresOtp = true
                };
            }
            catch (Exception)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again.",
                    RequiresOtp = false
                };
            }
        }

        public async Task<SignupResponseDto> SignupAsync(SignupRequestDto request)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new SignupResponseDto
                    {
                        Success = false,
                        Message = "Email already registered. Please try logging in.",
                        RequiresOtp = false
                    };
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    FullName = request.FullName,
                    IsEmailVerified = false,
                    IsActive = true
                };

                await _userRepository.AddAsync(user);

                // Generate and send OTP
                var otpCode = await _otpService.GenerateOtpAsync(request.Email, "SIGNUP", user.Id);
                var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode.Code, "SIGNUP");

                if (!emailSent)
                {
                    return new SignupResponseDto
                    {
                        Success = false,
                        Message = "Failed to send verification email. Please try again.",
                        RequiresOtp = false
                    };
                }

                await _unitOfWork.SaveChangesAsync();

                return new SignupResponseDto
                {
                    Success = true,
                    Message = "Account created successfully. Please verify your email with the OTP sent.",
                    RequiresOtp = true,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception)
            {
                return new SignupResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again.",
                    RequiresOtp = false
                };
            }
        }

        public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request)
        {
            try
            {
                var isValid = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode, request.Purpose);
                
                if (!isValid)
                {
                    return new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired OTP code."
                    };
                }

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    return new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Mark OTP as used
                var otp = await _otpService.GetValidOtpAsync(request.Email, request.Purpose);
                if (otp != null)
                {
                    await _otpService.MarkOtpAsUsedAsync(otp.Id);
                }

                // Handle different purposes
                if (request.Purpose == "SIGNUP")
                {
                    user.MarkEmailAsVerified();
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName ?? "User");
                }

                if (request.Purpose == "LOGIN" || request.Purpose == "SIGNUP")
                {
                    user.UpdateLastLogin();
                }

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var token = await GenerateJwtTokenAsync(user);

                return new VerifyOtpResponseDto
                {
                    Success = true,
                    Message = request.Purpose == "SIGNUP" ? "Email verified successfully. Welcome!" : "Login successful.",
                    Token = token,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception)
            {
                return new VerifyOtpResponseDto
                {
                    Success = false,
                    Message = "An error occurred during verification. Please try again."
                };
            }
        }

        public async Task<ResendOtpResponseDto> ResendOtpAsync(ResendOtpRequestDto request)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null && request.Purpose != "SIGNUP")
                {
                    return new ResendOtpResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Generate and send new OTP
                var otpCode = await _otpService.GenerateOtpAsync(request.Email, request.Purpose, user?.Id);
                var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode.Code, request.Purpose);

                if (!emailSent)
                {
                    return new ResendOtpResponseDto
                    {
                        Success = false,
                        Message = "Failed to send OTP. Please try again."
                    };
                }

                await _unitOfWork.SaveChangesAsync();

                return new ResendOtpResponseDto
                {
                    Success = true,
                    Message = "New OTP sent successfully."
                };
            }
            catch (Exception)
            {
                return new ResendOtpResponseDto
                {
                    Success = false,
                    Message = "An error occurred. Please try again."
                };
            }
        }

        public Task<string> GenerateJwtTokenAsync(User user)
        {
            var jwtSettings = _configuration.GetSection("ApiSettings:JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "RapidTickets";
            var audience = jwtSettings["Audience"] ?? "RapidTicketsApp";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim("EmailVerified", user.IsEmailVerified.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task LogoutAsync(Guid userId)
        {
            // In a JWT-based system, logout is primarily client-side (delete token)
            // But we can update the user's last activity for tracking purposes
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    // You could add a LastLogoutAt property to track logout time
                    // For now, we just update the record to trigger any audit trails
                    await _userRepository.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Logout should not fail even if database update fails
                // The token will still be invalid on the client side
            }
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IsEmailVerified = user.IsEmailVerified,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                if (!user.IsEmailVerified)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email not verified."
                    };
                }

                // Generate new JWT token
                var token = await GenerateJwtTokenAsync(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    Token = token,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during token refresh. Please try again."
                };
            }
        }

        public async Task<Result<UserDto>> GetUserProfileAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<UserDto>.Failure("User not found.");
                }

                var userDto = MapToUserDto(user);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception)
            {
                return Result<UserDto>.Failure("An error occurred while fetching user profile. Please try again.");
            }
        }
    }
}