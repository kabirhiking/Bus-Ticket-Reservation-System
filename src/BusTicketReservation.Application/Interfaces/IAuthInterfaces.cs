using BusTicketReservation.Application.DTOs;
using BusTicketReservation.Application.Common;
using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<SignupResponseDto> SignupAsync(SignupRequestDto request);
        Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request);
        Task<ResendOtpResponseDto> ResendOtpAsync(ResendOtpRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(Guid userId);
        Task<Result<UserDto>> GetUserProfileAsync(Guid userId);
        Task<string> GenerateJwtTokenAsync(User user);
        Task LogoutAsync(Guid userId);
    }

    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otpCode, string purpose);
        Task<bool> SendWelcomeEmailAsync(string email, string fullName);
    }

    public interface IOtpService
    {
        Task<OtpCode> GenerateOtpAsync(string email, string purpose, Guid? userId = null);
        Task<bool> ValidateOtpAsync(string email, string code, string purpose);
        Task<OtpCode?> GetValidOtpAsync(string email, string purpose);
        Task MarkOtpAsUsedAsync(Guid otpId);
        Task CleanupExpiredOtpsAsync();
        string GenerateRandomOtpCode();
    }

    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByIdWithTicketsAsync(Guid id);
    }

    public interface IOtpRepository : IEntityRepository<OtpCode>
    {
        Task<OtpCode?> GetValidOtpAsync(string email, string purpose);
        Task<List<OtpCode>> GetExpiredOtpsAsync();
        Task<int> GetOtpAttemptCountAsync(string email, string purpose, DateTime since);
    }
}