using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using System.Security.Cryptography;

namespace BusTicketReservation.Application.Services
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OtpService(IOtpRepository otpRepository, IUnitOfWork unitOfWork)
        {
            _otpRepository = otpRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OtpCode> GenerateOtpAsync(string email, string purpose, Guid? userId = null)
        {
            // Invalidate any existing OTPs for the same email and purpose
            var existingOtps = await _otpRepository.GetAllAsync();
            var userExistingOtps = existingOtps.Where(o => o.Email == email && o.Purpose == purpose && !o.IsUsed);
            
            foreach (var existingOtp in userExistingOtps)
            {
                existingOtp.IsUsed = true;
                await _otpRepository.UpdateAsync(existingOtp);
            }

            // Generate new OTP
            var otpCode = new OtpCode
            {
                Email = email,
                Code = GenerateRandomOtpCode(),
                Purpose = purpose,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10 minutes expiry
                IsUsed = false,
                AttemptCount = 0,
                MaxAttempts = 3
            };

            await _otpRepository.AddAsync(otpCode);
            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(string email, string code, string purpose)
        {
            var otp = await GetValidOtpAsync(email, purpose);
            
            if (otp == null)
            {
                return false;
            }

            // Increment attempt count
            otp.AttemptCount++;
            await _otpRepository.UpdateAsync(otp);

            // Check if code matches
            if (otp.Code != code)
            {
                // If max attempts reached, mark as used
                if (otp.AttemptCount >= otp.MaxAttempts)
                {
                    otp.IsUsed = true;
                    await _otpRepository.UpdateAsync(otp);
                }
                return false;
            }

            return otp.IsValid;
        }

        public async Task<OtpCode?> GetValidOtpAsync(string email, string purpose)
        {
            return await _otpRepository.GetValidOtpAsync(email, purpose);
        }

        public async Task MarkOtpAsUsedAsync(Guid otpId)
        {
            var otp = await _otpRepository.GetByIdAsync(otpId);
            if (otp != null)
            {
                otp.IsUsed = true;
                await _otpRepository.UpdateAsync(otp);
            }
        }

        public async Task CleanupExpiredOtpsAsync()
        {
            var expiredOtps = await _otpRepository.GetExpiredOtpsAsync();
            foreach (var otp in expiredOtps)
            {
                await _otpRepository.DeleteAsync(otp);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public string GenerateRandomOtpCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (randomNumber % 1000000).ToString("D6"); // 6-digit OTP
        }
    }
}