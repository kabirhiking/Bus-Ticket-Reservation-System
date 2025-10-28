using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.Entities
{
    public class OtpCode : BaseEntity, IAggregateRoot
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty; // LOGIN, SIGNUP, PASSWORD_RESET
        public bool IsUsed { get; set; } = false;
        public DateTime ExpiresAt { get; set; }
        public int AttemptCount { get; set; } = 0;
        public int MaxAttempts { get; set; } = 3;

        // Navigation properties
        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        public bool IsValid => !IsUsed && DateTime.UtcNow <= ExpiresAt && AttemptCount < MaxAttempts;

        public OtpCode()
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(10); // 10 minutes expiry
        }
    }
}